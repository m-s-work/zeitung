using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Integration;

/// <summary>
/// Light tests for HTML5 feed ingestion.
/// These tests validate that configured HTML5 feeds can be fetched and parsed using CSS selectors.
/// </summary>
[TestFixture]
[Category("IntegrationTest")]
public class HtmlFeedIngestLightTests
{
    private List<RssFeed> _htmlFeeds = new();
    private const string ArtifactsDir = "test-artifacts";

    [OneTimeSetUp]
    public void OneTimeSetUpAsync()
    {
        _htmlFeeds = ReadHtmlFeedConfig();
        
        // Create artifacts directory if it doesn't exist
        if (!Directory.Exists(ArtifactsDir))
        {
            Directory.CreateDirectory(ArtifactsDir);
        }
    }
    
    /// <summary>
    /// Saves content to an artifact file for debugging.
    /// </summary>
    private static void SaveArtifact(string feedName, string content, string extension = "html")
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
    
    public static IEnumerable<TestCaseData> HtmlFeedTestCases
    {
        get
        {
            var htmlFeeds = ReadHtmlFeedConfig();

            foreach (var feed in htmlFeeds)
            {
                yield return new TestCaseData(feed).SetArgDisplayNames(feed.Name.Replace(" ", "_").Replace(".", "_"));
            }
        }
    }

    private static List<RssFeed> ReadHtmlFeedConfig()
    {
        var feedsPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "feeds.json5");
        var feedsJson = File.ReadAllText(feedsPath);
        var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
        {
            ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
        };
        var allFeeds = System.Text.Json.JsonSerializer.Deserialize<List<RssFeed>>(feedsJson, jsonSerializerOptions) ?? new List<RssFeed>();
        
        // Expand feeds with URL patterns
        var expandedFeeds = FeedExpander.ExpandFeeds(allFeeds);
        
        // Filter only HTML5 feeds
        return expandedFeeds.Where(f => f.Type.Equals("html5", StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Tests that each configured HTML5 feed can be successfully fetched and parsed.
    /// This test validates:
    /// 1. The feed URL is accessible
    /// 2. The feed returns valid HTML content
    /// 3. The feed can be parsed successfully with CSS selectors
    /// 4. At least one article is extracted
    /// </summary>
    [TestCaseSource(nameof(HtmlFeedTestCases))]
    public async Task HtmlFeed_ShouldBeSuccessfullyIngested(RssFeed feed)
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
            Assert.Fail($"Failed to fetch HTML5 feed '{feed.Name}' from {feed.Url}. Error: {fetchException.Message}");
        }

        Assert.That(response, Is.Not.Null, $"Feed '{feed.Name}' should return a response");
        
        if (!response!.IsSuccessStatusCode)
        {
            content = await response.Content.ReadAsStringAsync();
            SaveArtifact($"{feed.Name}_error_{response.StatusCode}", content);
            var contentPreview = content.Length > 3000 ? content.Substring(0, 3000) + "..." : content;
            Assert.Fail($"Feed '{feed.Name}' returned error status code: {response.StatusCode}. Full page saved to artifacts. Content preview: {contentPreview}");
        }

        content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.Not.Empty, $"Feed '{feed.Name}' should return non-empty content");
        
        // Verify content looks like HTML
        var trimmedContent = content.TrimStart();
        if (!trimmedContent.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) && 
            !trimmedContent.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
        {
            SaveArtifact($"{feed.Name}_not_html", content);
            var contentPreview = content.Length > 5000 ? content.Substring(0, 5000) + "..." : content;
            Assert.Inconclusive($"Feed '{feed.Name}' may not be returning valid HTML content. Full page saved to artifacts. Content starts with: {contentPreview}");
        }
        
        // Test parsing the feed with HtmlFeedParser
        var parser = new HtmlFeedParser(
            new MockHttpClientFactory(content),
            new MockLogger<HtmlFeedParser>());
        
        var articles = await parser.ParseFeedAsync(feed);
        
        Assert.That(articles, Is.Not.Null, $"Feed '{feed.Name}' should be parseable");
        
        if (articles.Count == 0)
        {
            SaveArtifact($"{feed.Name}_no_articles", content);
            var contentPreview = content.Length > 5000 ? content.Substring(0, 5000) + "..." : content;
            Assert.Fail($"Feed '{feed.Name}' parsed successfully but contained no articles. Selectors may need adjustment. Full page saved to artifacts. HTML preview: {contentPreview}");
        }
        
        // Verify article structure
        var firstArticle = articles.First();
        Assert.That(firstArticle.Title, Is.Not.Null.And.Not.Empty, $"Feed '{feed.Name}' articles should have titles");
        Assert.That(firstArticle.Link, Is.Not.Null.And.Not.Empty, $"Feed '{feed.Name}' articles should have links");
        Assert.That(firstArticle.FeedSource, Is.EqualTo(feed.Name), $"Feed '{feed.Name}' articles should have correct source");
    }

    [Test]
    public async Task AllConfiguredHtmlFeeds_ShouldBeAccessible()
    {
        // Arrange
        var failedFeeds = new List<(RssFeed feed, string error)>();

        // Act
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        foreach (var feed in _htmlFeeds)
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
            var errorMessage = "The following HTML5 feeds failed:\n" + 
                string.Join("\n", failedFeeds.Select(f => $"  - {f.feed.Name} ({f.feed.Url}): {f.error}"));
            Assert.Fail(errorMessage);
        }

        Assert.Pass($"All {_htmlFeeds.Count} configured HTML5 feeds are accessible");
    }

    [Test]
    public void HtmlFeedConfiguration_ShouldBeLoaded()
    {
        // Assert
        Assert.That(_htmlFeeds, Is.Not.Null, "HTML5 feeds should be loaded from configuration");
        Assert.That(_htmlFeeds, Is.Not.Empty, "At least one HTML5 feed should be configured");
        
        foreach (var feed in _htmlFeeds)
        {
            Assert.That(feed.Name, Is.Not.Null.And.Not.Empty, "Feed name should be configured");
            Assert.That(feed.Url, Is.Not.Null.And.Not.Empty, "Feed URL should be configured");
            Assert.That(() => new Uri(feed.Url), Throws.Nothing, $"Feed URL '{feed.Url}' should be valid");
            Assert.That(feed.HtmlConfig, Is.Not.Null, $"HTML5 feed '{feed.Name}' should have HtmlConfig");
            Assert.That(feed.HtmlConfig!.ItemsSelector, Is.Not.Null.And.Not.Empty, 
                $"HTML5 feed '{feed.Name}' should have ItemsSelector");
        }
    }

    [Test]
    public void UrlPatternExpansion_ShouldWork()
    {
        // Arrange
        var feedWithPattern = new RssFeed
        {
            Name = "Test {pattern}",
            Url = "https://example.com/{pattern}/",
            Type = "html5",
            UrlPatterns = new List<string> { "cat1", "cat2", "cat3" },
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article",
                Title = new SelectorConfig { Selector = "h2", Extractor = "text" },
                Link = new SelectorConfig { Selector = "a", Extractor = "href" }
            }
        };

        // Act
        var expanded = FeedExpander.ExpandFeed(feedWithPattern);

        // Assert
        Assert.That(expanded, Has.Count.EqualTo(3));
        Assert.That(expanded[0].Name, Is.EqualTo("Test cat1"));
        Assert.That(expanded[0].Url, Is.EqualTo("https://example.com/cat1/"));
        Assert.That(expanded[1].Name, Is.EqualTo("Test cat2"));
        Assert.That(expanded[1].Url, Is.EqualTo("https://example.com/cat2/"));
        Assert.That(expanded[2].Name, Is.EqualTo("Test cat3"));
        Assert.That(expanded[2].Url, Is.EqualTo("https://example.com/cat3/"));
    }
}
