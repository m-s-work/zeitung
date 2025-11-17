using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeitung.AppHost.Tests.TestHelpers;
using Zeitung.Core.Context;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;
using Zeitung.Worker.Strategies;

namespace Zeitung.Worker.Tests.Integration;

/// <summary>
/// Integration tests for RSS feed ingestion.
/// These tests validate that configured RSS feeds can be fetched, parsed, and ingested into the database.
/// </summary>
[TestFixture]
//[Category("IntegrationTest")] // this is no integration test since we use in-memory db
[Category("Db")]
public class RssFeedIntegrationTests// : AspireIntegrationTestBase
{
    private List<RssFeed> _rssFeeds = new();
    private const string TestDbName = "RssFeedIntegrationTests";

    //[OneTimeSetUp, CancelAfter(30_000)]
    //public new async Task OneTimeSetUpAsync(CancellationToken cancellationToken)
    //{
    //    _rssFeeds = ReadRssFeedConfig();
    //    await base.OneTimeSetUpAsync(cancellationToken);
    //}

    [OneTimeSetUp]
    public void OneTimeSetUpAsync()
    {
        _rssFeeds = ReadRssFeedConfig();
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
        
        // Expand feeds with URL patterns
        return FeedExpander.ExpandFeeds(rssFeeds);
    }

    /// <summary>
    /// Provides test cases for each RSS feed configured in feeds.json
    /// </summary>
    public static IEnumerable<TestCaseData> RssFeedTestCases
    {
        get
        {
            var rssFeeds = ReadRssFeedConfig();
            
            // Filter out HTML5 feeds - they have their own tests in HtmlFeedIngestLightTests
            foreach (var feed in rssFeeds.Where(f => f.Type == "rss" || f.Type == "rdf"))
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
        
        // Test database ingestion using the real FeedIngestService
        var dbContext = CreateInMemoryDbContext();
        try
        {
            // Create service with real dependencies but in-memory database
            var rdfParser = new RdfFeedParser(new MockLogger<RdfFeedParser>());
            var rssParser = new RssFeedParser(
                new MockHttpClientFactory(content),
                new MockLogger<RssFeedParser>(),
                rdfParser);
            var htmlParser = new HtmlFeedParser(
                new MockHttpClientFactory(content),
                new MockLogger<HtmlFeedParser>());
            var parsers = new IFeedParser[] { rssParser, htmlParser };
            var parserFactory = new FeedParserFactory(parsers, new MockLogger<FeedParserFactory>());
            var articleRepository = new ArticleRepository(dbContext, new MockLogger<ArticleRepository>());
            var tagRepository = new PostgresTagRepository(dbContext, new MockLogger<PostgresTagRepository>());
            var taggingStrategy = new MockTaggingStrategy();
            var elasticsearchService = new MockElasticsearchService();
            var feedOptions = Microsoft.Extensions.Options.Options.Create(new RssFeedOptions());
            
            var feedIngestService = new FeedIngestService(
                parserFactory,
                taggingStrategy,
                articleRepository,
                tagRepository,
                elasticsearchService,
                new MockLogger<FeedIngestService>(),
                feedOptions);

            // Act - Ingest the feed using the real service
            await feedIngestService.IngestFeedAsync(feed);

            // Assert - Verify articles were persisted
            var articles = await dbContext.Articles.ToListAsync();
            Assert.That(articles, Is.Not.Empty, $"Articles from feed '{feed.Name}' should be saved to database");

            // Verify first article details
            var firstArticle = articles.First();
            Assert.That(firstArticle.Id, Is.GreaterThan(0), "Saved article should have a valid ID");
            Assert.That(firstArticle.Title, Is.Not.Empty, "Article should have a title");
            Assert.That(firstArticle.Link, Is.Not.Empty, "Article should have a link");

            // Verify tags were saved
            var savedTags = await dbContext.ArticleTags
                .Where(at => at.ArticleId == firstArticle.Id)
                .Include(at => at.Tag)
                .ToListAsync();
            
            Assert.That(savedTags, Is.Not.Empty, "Tags should be saved for the article");
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
