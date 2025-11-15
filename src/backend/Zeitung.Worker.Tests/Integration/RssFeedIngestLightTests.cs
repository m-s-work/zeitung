using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Integration;

/// <summary>
/// Light tests for RSS feed ingestion.
/// These tests validate that configured RSS feeds can be fetched and ingested into the database.
/// </summary>
[TestFixture]
[Category("IntegrationTest")]
public class RssFeedIngestLightTests
{
    private List<RssFeed> _rssFeeds = new();
    private const string ArtifactsDir = "test-artifacts";

    [OneTimeSetUp]
    public void OneTimeSetUpAsync()
    {
        _rssFeeds = ReadRssFeedConfig();
        
        // Create artifacts directory if it doesn't exist
        if (!Directory.Exists(ArtifactsDir))
        {
            Directory.CreateDirectory(ArtifactsDir);
        }
    }
    
    /// <summary>
    /// Saves content to an artifact file for debugging.
    /// </summary>
    private static void SaveArtifact(string feedName, string content, string extension = "xml")
    {
        try
        {
            var sanitizedName = string.Join("_", feedName.Split(Path.GetInvalidFileNameChars()));
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var filename = $"{sanitizedName}_{timestamp}.{extension}";
            var filepath = Path.Combine(ArtifactsDir, filename);
            
            File.WriteAllText(filepath, content);
            TestContext.Out.WriteLine($"Artifact saved: {filepath}");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Failed to save artifact: {ex.Message}");
        }
    }
    
    public static IEnumerable<TestCaseData> RssFeedTestCases
    {
        get
        {
            var rssFeeds = ReadRssFeedConfig();

            foreach (var feed in rssFeeds)
            {
                yield return new TestCaseData(feed).SetArgDisplayNames(feed.Name.Replace(" ", "_").Replace(".", "_"));
            }
        }
    }

    private static List<RssFeed> ReadRssFeedConfig()
    {
        var feedsPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "feeds.json5");
        var feedsJson = File.ReadAllText(feedsPath);
        //var rssFeeds = System.Text.Json.JsonSerializer.Deserialize<List<RssFeed>>(feedsJson) ?? new List<RssFeed>();
        var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
        {
            ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
        };
        var allFeeds = System.Text.Json.JsonSerializer.Deserialize<List<RssFeed>>(feedsJson, jsonSerializerOptions) ?? new List<RssFeed>();
        
        // Filter only RSS/RDF feeds (exclude HTML5 feeds)
        var rssFeeds = allFeeds.Where(f => !f.Type.Equals("html5", StringComparison.OrdinalIgnoreCase)).ToList();
        
        return rssFeeds;
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
            SaveArtifact($"{feed.Name}_error_{response.StatusCode}", content);
            var contentPreview = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
            Assert.Fail($"Feed '{feed.Name}' returned error status code: {response.StatusCode}. Full content saved to artifacts. Content preview: {contentPreview}");
        }

        content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.Not.Empty, $"Feed '{feed.Name}' should return non-empty content");
        
        // Verify content looks like RSS/XML
        var trimmedContent = content.TrimStart();
        if (!trimmedContent.StartsWith("<?xml") && !trimmedContent.StartsWith("<rss") && !trimmedContent.StartsWith("<feed"))
        {
            SaveArtifact($"{feed.Name}_not_xml", content);
            var contentPreview = content.Length > 1000 ? content.Substring(0, 1000) + "..." : content;
            Assert.Fail($"Feed '{feed.Name}' did not return valid XML/RSS content. Full content saved to artifacts. Content starts with: {contentPreview}");
        }
        
        // Test parsing the feed
        var rdfParser = new RdfFeedParser(new MockLogger<RdfFeedParser>());
        var parser = new RssFeedParser(
            new MockHttpClientFactory(content),
            new MockLogger<RssFeedParser>(),
            rdfParser);
        
        var articles = await parser.ParseFeedAsync(feed);
        
        Assert.That(articles, Is.Not.Null, $"Feed '{feed.Name}' should be parseable");
        
        if (articles.Count == 0)
        {
            SaveArtifact($"{feed.Name}_no_articles", content);
            var contentPreview = content.Length > 1000 ? content.Substring(0, 1000) + "..." : content;
            Assert.Fail($"Feed '{feed.Name}' parsed successfully but contained no articles. Full XML saved to artifacts. Raw XML preview: {contentPreview}");
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
