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
[Ignore("same as Lite tests, but slower startup time")] // TODO we need to change/refactor it so here full ingest into DB is done
// TODO also cleanup stuff like in light tests (rss feed config read)
public class RssFeedIntegrationTests
{
    private IDistributedApplicationTestingBuilder? _builder;
    private DistributedApplication? _app;
    private List<RssFeed> _rssFeeds = new();

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        // Load RSS feeds from separate feeds.json file
        var feedsPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "feeds.json5");
        var feedsJson = await File.ReadAllTextAsync(feedsPath);
        _rssFeeds = System.Text.Json.JsonSerializer.Deserialize<List<RssFeed>>(feedsJson) ?? new List<RssFeed>();

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Zeitung_AppHost>();

        // Configure HTTP client resilience/timeouts
        //appHost.Services.ConfigureHttpClientDefaults(http =>
        //{
        //    http.AddStandardResilienceHandler(options =>
        //    {
        //        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);
        //        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
        //        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
        //    });
        //});

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
    /// Provides test cases for each RSS feed configured in feeds.json
    /// </summary>
    public static IEnumerable<TestCaseData> RssFeedTestCases
    {
        get
        {
            // Load RSS feeds from separate feeds.json file
            var feedsPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "feeds.json");
            var feedsJson = File.ReadAllText(feedsPath);
            var rssFeeds = System.Text.Json.JsonSerializer.Deserialize<List<RssFeed>>(feedsJson) ?? new List<RssFeed>();
            
            foreach (var feed in rssFeeds)
            {
                yield return new TestCaseData(feed)
                    .SetArgDisplayNames(feed.Name.Replace(" ", "_").Replace(".", "_"))
                    //.SetName($"RssFeed_ShouldBeIngestible_{feed.Name.Replace(" ", "_").Replace(".", "_")}")
                    ;
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
        string? content = null;
        
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
        
        if (!response!.IsSuccessStatusCode)
        {
            content = await response.Content.ReadAsStringAsync();
            var contentPreview = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
            Assert.Fail($"Feed '{feed.Name}' returned error status code: {response.StatusCode}. Content preview: {contentPreview}");
        }

        content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.Not.Empty, $"Feed '{feed.Name}' should return non-empty content");
        
        // Verify content looks like RSS/XML
        var trimmedContent = content.TrimStart();
        if (!trimmedContent.StartsWith("<?xml") && !trimmedContent.StartsWith("<rss") && !trimmedContent.StartsWith("<feed"))
        {
            var contentPreview = content.Length > 1000 ? content.Substring(0, 1000) + "..." : content;
            Assert.Fail($"Feed '{feed.Name}' did not return valid XML/RSS content. Content starts with: {contentPreview}");
        }
        
        // Test parsing the feed
        try
        {
            var rdfParser = new RdfFeedParser(new MockLogger<RdfFeedParser>());
            var parser = new RssFeedParser(
                new MockHttpClientFactory(content),
                new MockLogger<RssFeedParser>(),
                rdfParser);
            
            var articles = await parser.ParseFeedAsync(feed);
            
            Assert.That(articles, Is.Not.Null, $"Feed '{feed.Name}' should be parseable");
            
            if (articles.Count == 0)
            {
                var contentPreview = content.Length > 1000 ? content.Substring(0, 1000) + "..." : content;
                Assert.Fail($"Feed '{feed.Name}' parsed successfully but contained no articles. Raw XML preview: {contentPreview}");
            }
        }
        catch (Exception ex)
        {
            var contentPreview = content.Length > 1000 ? content.Substring(0, 1000) + "..." : content;
            Assert.Fail($"Failed to parse RSS feed '{feed.Name}'. Error: {ex.Message}\n\nRaw XML preview: {contentPreview}");
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
