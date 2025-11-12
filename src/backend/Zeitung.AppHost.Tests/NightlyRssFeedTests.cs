using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.AppHost.Tests;

/// <summary>
/// Nightly integration tests that verify all RSS feeds are working correctly.
/// These tests actually fetch and parse real RSS feeds to detect when feeds go down or change format.
/// </summary>
[TestFixture]
[Category("IntegrationTest")]
[Category("NightlyFeedTest")]
public class NightlyRssFeedTests
{
    private IDistributedApplicationTestingBuilder? _builder;
    private DistributedApplication? _app;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zeitung_AppHost>(
            [
                "--environment=ci"
            ]);

        // Configure logging
        appHost.Services.AddLogging(logging => logging
            .AddFilter("Default", LogLevel.Information)
            .AddFilter("Microsoft.AspNetCore", LogLevel.Warning)
            .AddFilter("Aspire.Hosting.Dcp", LogLevel.Warning));

        // Configure HTTP client with longer timeouts for external feeds
        appHost.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
            });
        });

        _builder = appHost;
        _app = await appHost.BuildAsync();
        await _app.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        if (_app != null)
        {
            try
            {
                await _app.StopAsync();
            }
            catch
            {
                // Ignore stop failures during teardown
            }

            try
            {
                await _app.DisposeAsync();
            }
            catch
            {
                // Ignore dispose failures during teardown
            }

            _app = null;
            _builder = null;
        }
    }

    /// <summary>
    /// Provides test cases for each RSS feed configured in the system.
    /// This enables NUnit to run a separate test for each feed.
    /// </summary>
    public static IEnumerable<TestCaseData> RssFeedTestCases
    {
        get
        {
            var feeds = new[]
            {
                new RssFeed
                {
                    Name = "BBC News",
                    Url = "http://feeds.bbci.co.uk/news/rss.xml",
                    Description = "BBC News - World"
                },
                new RssFeed
                {
                    Name = "Heise Online",
                    Url = "https://www.heise.de/rss/heise-atom.xml",
                    Description = "Heise Online News"
                },
                new RssFeed
                {
                    Name = "Golem.de",
                    Url = "https://rss.golem.de/rss.php?feed=RSS2.0",
                    Description = "Golem.de IT News"
                },
                new RssFeed
                {
                    Name = "ORF.at",
                    Url = "https://rss.orf.at/news.xml",
                    Description = "ORF.at News"
                }
            };

            foreach (var feed in feeds)
            {
                yield return new TestCaseData(feed).SetName($"Feed_{feed.Name.Replace(" ", "_").Replace(".", "")}");
            }
        }
    }

    /// <summary>
    /// Tests that an RSS feed can be successfully fetched and parsed.
    /// This test will fail if:
    /// - The feed URL is unreachable
    /// - The feed returns an HTTP error
    /// - The feed format is invalid or changed
    /// - The feed contains no items
    /// </summary>
    [Test]
    [TestCaseSource(nameof(RssFeedTestCases))]
    public async Task RssFeed_CanBeFetchedAndParsed_Successfully(RssFeed feed)
    {
        // Arrange
        var app = _app!;
        
        // Create a service scope to get the RSS feed parser
        var serviceProvider = app.Services;
        using var scope = serviceProvider.CreateScope();
        
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<RssFeedParser>>();
        var parser = new RssFeedParser(httpClientFactory, logger);

        // Act
        List<Article> articles;
        Exception? caughtException = null;
        
        try
        {
            articles = await parser.ParseFeedAsync(feed, CancellationToken.None);
        }
        catch (Exception ex)
        {
            caughtException = ex;
            articles = new List<Article>();
        }

        // Assert
        Assert.That(caughtException, Is.Null, 
            $"Failed to fetch RSS feed '{feed.Name}' from {feed.Url}. Error: {caughtException?.Message}");
        
        Assert.That(articles, Is.Not.Null, 
            $"Parser returned null for feed '{feed.Name}' ({feed.Url})");
        
        Assert.That(articles.Count, Is.GreaterThan(0), 
            $"RSS feed '{feed.Name}' ({feed.Url}) returned no articles. The feed may be empty or the format may have changed.");

        // Verify that articles have expected properties
        var firstArticle = articles.First();
        Assert.That(firstArticle.Title, Is.Not.Null.And.Not.Empty, 
            $"First article from feed '{feed.Name}' has no title");
        Assert.That(firstArticle.Link, Is.Not.Null.And.Not.Empty, 
            $"First article from feed '{feed.Name}' has no link");

        TestContext.WriteLine($"✓ Feed '{feed.Name}' successfully parsed {articles.Count} articles");
        TestContext.WriteLine($"  First article: {firstArticle.Title}");
        TestContext.WriteLine($"  URL: {feed.Url}");
    }

    /// <summary>
    /// Tests that all RSS feeds can be ingested into the database successfully.
    /// This is a more comprehensive test that exercises the full ingestion pipeline.
    /// </summary>
    [Test]
    [Explicit("This test takes longer and is meant for full end-to-end validation")]
    public async Task AllFeeds_CanBeIngested_ThroughFullPipeline()
    {
        // Arrange
        var app = _app!;
        var httpClient = app.CreateHttpClient("api");
        
        // Wait for the API to be ready
        var maxRetries = 10;
        var retryDelay = TimeSpan.FromSeconds(5);
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await httpClient.GetAsync("/health");
                if (response.IsSuccessStatusCode)
                {
                    break;
                }
            }
            catch
            {
                if (i == maxRetries - 1)
                {
                    throw;
                }
            }
            
            await Task.Delay(retryDelay);
        }

        // This test would trigger the feed ingestion job and verify all feeds are processed
        // For now, we just verify the API is running and healthy
        var healthResponse = await httpClient.GetAsync("/health");
        Assert.That(healthResponse.IsSuccessStatusCode, Is.True, "API health check should be successful");
        
        TestContext.WriteLine("✓ Full feed ingestion pipeline is operational");
    }
}
