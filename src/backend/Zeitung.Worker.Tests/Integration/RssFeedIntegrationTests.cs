using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeitung.AppHost.Tests.TestHelpers;
using Zeitung.Core.Models;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;
using Zeitung.Worker.Strategies;

namespace Zeitung.Worker.Tests.Integration;

/// <summary>
/// Integration tests for RSS feed ingestion.
/// These tests validate that configured RSS feeds can be fetched, parsed, and ingested into the database.
/// </summary>
[TestFixture]
[Category("IntegrationTest")]
public class RssFeedIntegrationTests : AspireIntegrationTestBase
{
    private List<RssFeed> _rssFeeds = new();
    private const string TestDbName = "RssFeedIntegrationTests";

    [OneTimeSetUp]
    public new async Task OneTimeSetUpAsync()
    {
        _rssFeeds = ReadRssFeedConfig();
        await base.OneTimeSetUpAsync();
    }

    private static List<RssFeed> ReadRssFeedConfig()
    {
        var feedsPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "feeds.json5");
        var feedsJson = File.ReadAllText(feedsPath);
        var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
        {
            ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
        };
        var rssFeeds = System.Text.Json.JsonSerializer.Deserialize<List<RssFeed>>(feedsJson, jsonSerializerOptions) ?? new List<RssFeed>();
        return rssFeeds;
    }

    /// <summary>
    /// Provides test cases for each RSS feed configured in feeds.json
    /// </summary>
    public static IEnumerable<TestCaseData> RssFeedTestCases
    {
        get
        {
            var rssFeeds = ReadRssFeedConfig();
            
            foreach (var feed in rssFeeds)
            {
                yield return new TestCaseData(feed)
                    .SetArgDisplayNames(feed.Name.Replace(" ", "_").Replace(".", "_"));
            }
        }
    }

    /// <summary>
    /// Tests that each configured RSS feed can be successfully fetched, parsed, and ingested into the database.
    /// This test validates:
    /// 1. The feed URL is accessible
    /// 2. The feed returns valid XML/RSS content
    /// 3. The feed can be parsed successfully
    /// 4. Articles can be saved to the database
    /// 5. Tags are generated and associated with articles
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
        List<Zeitung.Worker.Models.Article> articles;
        try
        {
            var rdfParser = new RdfFeedParser(new MockLogger<RdfFeedParser>());
            var parser = new RssFeedParser(
                new MockHttpClientFactory(content),
                new MockLogger<RssFeedParser>(),
                rdfParser);
            
            articles = await parser.ParseFeedAsync(feed);
            
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
            return; // Ensure articles is not used below
        }

        // Test database ingestion
        var dbContext = CreateInMemoryDbContext();
        var articleRepository = new ArticleRepository(dbContext, new MockLogger<ArticleRepository>());
        var tagRepository = new PostgresTagRepository(dbContext, new MockLogger<PostgresTagRepository>());
        var taggingStrategy = new MockTaggingStrategy();

        try
        {
            // Process first article from the feed
            var article = articles.First();
            
            // Generate tags
            article.Tags = await taggingStrategy.GenerateTagsAsync(article);
            Assert.That(article.Tags, Is.Not.Empty, $"Tags should be generated for article '{article.Title}'");

            // Save article to database
            var savedArticle = await articleRepository.SaveAsync(article);
            Assert.That(savedArticle, Is.Not.Null, $"Article '{article.Title}' should be saved to database");
            Assert.That(savedArticle.Id, Is.GreaterThan(0), "Saved article should have a valid ID");

            // Save tags
            await tagRepository.SaveArticleTagsAsync(savedArticle.Id, article.Tags);

            // Verify article was saved correctly
            var retrievedArticle = await articleRepository.GetByLinkAsync(article.Link);
            Assert.That(retrievedArticle, Is.Not.Null, "Article should be retrievable from database");
            Assert.That(retrievedArticle!.Title, Is.EqualTo(article.Title), "Retrieved article title should match");
            Assert.That(retrievedArticle.Link, Is.EqualTo(article.Link), "Retrieved article link should match");

            // Verify tags were saved
            var savedTags = await dbContext.ArticleTags
                .Where(at => at.ArticleId == savedArticle.Id)
                .Include(at => at.Tag)
                .ToListAsync();
            
            Assert.That(savedTags, Is.Not.Empty, "Tags should be saved for the article");
            Assert.That(savedTags.Count, Is.EqualTo(article.Tags.Count), "Number of saved tags should match");
        }
        finally
        {
            await dbContext.DisposeAsync();
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

    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    private ZeitungDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ZeitungDbContext>()
            .UseInMemoryDatabase(databaseName: $"{TestDbName}_{Guid.NewGuid()}")
            .Options;

        var context = new ZeitungDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
