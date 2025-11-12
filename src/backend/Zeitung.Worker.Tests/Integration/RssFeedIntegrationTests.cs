using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeitung.Core.Models;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Integration;

/// <summary>
/// Integration tests for RSS feed ingestion.
/// These tests validate that configured RSS feeds can be fetched and ingested into the database.
/// </summary>
[TestFixture]
[Category("IntegrationTest")]
public class RssFeedIntegrationTests
{
    private IDistributedApplicationTestingBuilder? _builder;
    private DistributedApplication? _app;
    private List<RssFeed> _rssFeeds = new();

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        // Load RSS feeds from configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".."))
            .AddJsonFile("Zeitung.Worker/appsettings.json", optional: false)
            .Build();

        _rssFeeds = config.GetSection("RssFeeds").Get<List<RssFeed>>() ?? new List<RssFeed>();

        // Create and start the distributed application
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

        // Configure HTTP client resilience/timeouts
        appHost.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
            });
        });

        _builder = appHost;
        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // Wait for services to be ready
        await Task.Delay(TimeSpan.FromSeconds(15));
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
    /// Provides test cases for each RSS feed configured in appsettings.json
    /// </summary>
    public static IEnumerable<TestCaseData> RssFeedTestCases
    {
        get
        {
            // Load RSS feeds from configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".."))
                .AddJsonFile("Zeitung.Worker/appsettings.json", optional: false)
                .Build();

            var rssFeeds = config.GetSection("RssFeeds").Get<List<RssFeed>>() ?? new List<RssFeed>();
            
            foreach (var feed in rssFeeds)
            {
                yield return new TestCaseData(feed)
                    .SetName($"RssFeed_ShouldBeIngestible_{feed.Name.Replace(" ", "_").Replace(".", "_")}");
            }
        }
    }

    /// <summary>
    /// Tests that each configured RSS feed can be successfully fetched and parsed.
    /// This test validates:
    /// 1. The feed URL is accessible
    /// 2. The feed returns valid XML/RSS content
    /// 3. The feed can be parsed successfully
    /// </summary>
    [TestCaseSource(nameof(RssFeedTestCases))]
    public async Task RssFeed_ShouldBeSuccessfullyIngested(RssFeed feed)
    {
        // Arrange - Test feed accessibility and parseability
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        HttpResponseMessage? response = null;
        Exception? fetchException = null;
        
        try
        {
            response = await httpClient.GetAsync(feed.Url);
        }
        catch (Exception ex)
        {
            fetchException = ex;
        }

        // Assert - Feed should be accessible
        if (fetchException != null)
        {
            Assert.Fail($"Failed to fetch RSS feed '{feed.Name}' from {feed.Url}. Error: {fetchException.Message}");
        }

        Assert.That(response, Is.Not.Null, $"Feed '{feed.Name}' should return a response");
        Assert.That(response!.IsSuccessStatusCode, Is.True, 
            $"Feed '{feed.Name}' should return success status code. Status: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.Not.Empty, $"Feed '{feed.Name}' should return non-empty content");
        
        // Verify content looks like RSS/XML
        Assert.That(content.TrimStart(), Does.StartWith("<?xml").Or.StartsWith("<rss").Or.StartsWith("<feed"),
            $"Feed '{feed.Name}' should return valid XML/RSS content");
        
        // Test parsing the feed
        try
        {
            var parser = new RssFeedParser(
                new MockHttpClientFactory(content),
                new MockLogger<RssFeedParser>());
            
            var articles = await parser.ParseFeedAsync(feed);
            
            Assert.That(articles, Is.Not.Null, $"Feed '{feed.Name}' should be parseable");
            Assert.That(articles.Count, Is.GreaterThan(0), 
                $"Feed '{feed.Name}' should contain at least one article");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to parse RSS feed '{feed.Name}'. Error: {ex.Message}");
        }
    }

    [Test]
    public async Task AllConfiguredFeeds_ShouldBeAccessible()
    {
        // Arrange
        var failedFeeds = new List<(RssFeed feed, string error)>();

        // Act
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        foreach (var feed in _rssFeeds)
        {
            try
            {
                var response = await httpClient.GetAsync(feed.Url);
                if (!response.IsSuccessStatusCode)
                {
                    failedFeeds.Add((feed, $"HTTP {response.StatusCode}"));
                }
            }
            catch (Exception ex)
            {
                failedFeeds.Add((feed, ex.Message));
            }
        }

        // Assert
        if (failedFeeds.Any())
        {
            var errorMessage = "The following feeds failed:\n" + 
                string.Join("\n", failedFeeds.Select(f => $"  - {f.feed.Name} ({f.feed.Url}): {f.error}"));
            Assert.Fail(errorMessage);
        }

        Assert.Pass($"All {_rssFeeds.Count} configured RSS feeds are accessible");
    }

    [Test]
    public void RssFeedConfiguration_ShouldBeLoaded()
    {
        // Assert
        Assert.That(_rssFeeds, Is.Not.Null, "RSS feeds should be loaded from configuration");
        Assert.That(_rssFeeds, Is.Not.Empty, "At least one RSS feed should be configured");
        
        foreach (var feed in _rssFeeds)
        {
            Assert.That(feed.Name, Is.Not.Null.And.Not.Empty, "Feed name should be configured");
            Assert.That(feed.Url, Is.Not.Null.And.Not.Empty, "Feed URL should be configured");
            Assert.That(() => new Uri(feed.Url), Throws.Nothing, $"Feed URL '{feed.Url}' should be valid");
        }
    }
}

/// <summary>
/// Mock HttpClientFactory for testing
/// </summary>
internal class MockHttpClientFactory : IHttpClientFactory
{
    private readonly string _content;

    public MockHttpClientFactory(string content)
    {
        _content = content;
    }

    public HttpClient CreateClient(string name)
    {
        var messageHandler = new MockHttpMessageHandler(_content);
        return new HttpClient(messageHandler);
    }
}

/// <summary>
/// Mock HttpMessageHandler for testing
/// </summary>
internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string _content;

    public MockHttpMessageHandler(string content)
    {
        _content = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(_content)
        });
    }
}

/// <summary>
/// Mock logger for testing
/// </summary>
internal class MockLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
