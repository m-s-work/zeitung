using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zeitung.Core.Models;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;
using Zeitung.Worker.Strategies;
using Zeitung.Worker.Tests.Integration;
using Zeitung.Worker.Tests.TestHelpers;

namespace Zeitung.Worker.Tests.Integration;

/// <summary>
/// Integration tests for the Worker service using AspireWorkerFactory.
/// Tests the full feed ingestion cycle with the complete DI container.
/// </summary>
[TestFixture]
[Category("Integration")]
public class WorkerIntegrationTests
{
    private AspireWorkerFactory? _factory;

    [TearDown]
    public async Task TearDown()
    {
        if (_factory != null)
        {
            await _factory.DisposeAsync();
            _factory = null;
        }
    }

    [Test]
    public async Task FeedIngestService_ShouldRunFullImportCycle_WithMockFeeds()
    {
        // Arrange - Build the Worker with mock test feeds
        _factory = await new AspireWorkerFactory().BuildAsync(builder =>
        {
            // Configure with mock RSS feeds to avoid hitting real endpoints
            builder.Configuration["TaggingStrategy"] = "Mock";
            
            // Override RSS feeds with mock test data
            builder.Services.Configure<RssFeedOptions>(options =>
            {
                options.Feeds = new List<RssFeed>
                {
                    new()
                    {
                        Name = "Mock BBC News",
                        Url = "http://example.com/mock-feed.xml",
                        Description = "Mock feed for testing",
                        Type = "rss"
                    }
                };
            });
            
            // Replace HTTP client factory with mock to return test XML
            var testXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<rss version=""2.0"">
    <channel>
        <title>Mock Test Feed</title>
        <link>http://example.com</link>
        <description>Mock feed for integration tests</description>
        <item>
            <title>Test Article 1</title>
            <link>http://example.com/article1</link>
            <description>This is a test article</description>
            <pubDate>Wed, 01 Jan 2025 12:00:00 GMT</pubDate>
        </item>
        <item>
            <title>Test Article 2</title>
            <link>http://example.com/article2</link>
            <description>This is another test article</description>
            <pubDate>Wed, 01 Jan 2025 13:00:00 GMT</pubDate>
        </item>
    </channel>
</rss>";
            
            builder.Services.AddSingleton<IHttpClientFactory>(new MockHttpClientFactory(testXml));
            
            // Use mock Elasticsearch service to avoid needing real Elasticsearch
            builder.Services.AddSingleton<IElasticsearchService, MockElasticsearchService>();
        });

        // Act - Run the full feed ingestion cycle
        using var scope = _factory.Services.CreateScope();
        var feedIngestService = scope.ServiceProvider.GetRequiredService<IFeedIngestService>();
        
        await feedIngestService.IngestFeedsAsync(CancellationToken.None);

        // Assert - Verify articles were persisted to the database
        var dbContext = scope.ServiceProvider.GetRequiredService<ZeitungDbContext>();
        var articles = await dbContext.Articles.ToListAsync();
        
        Assert.That(articles, Is.Not.Empty, "Articles should be saved to database");
        Assert.That(articles.Count, Is.EqualTo(2), "Should have 2 articles from the mock feed");
        
        var firstArticle = articles.First();
        Assert.That(firstArticle.Title, Is.EqualTo("Test Article 1"), "Article title should match");
        Assert.That(firstArticle.Link, Is.EqualTo("http://example.com/article1"), "Article link should match");
        
        // Verify tags were saved
        var tagsCount = await dbContext.ArticleTags.CountAsync();
        Assert.That(tagsCount, Is.GreaterThan(0), "Tags should be created for articles");
    }

    [Test]
    public async Task FeedIngestService_ShouldHandleEmptyFeedList()
    {
        // Arrange - Build the Worker with no feeds configured
        _factory = await new AspireWorkerFactory().BuildAsync(builder =>
        {
            builder.Configuration["TaggingStrategy"] = "Mock";
            
            builder.Services.Configure<RssFeedOptions>(options =>
            {
                options.Feeds = new List<RssFeed>();
            });
        });

        // Act - Run the feed ingestion with no feeds
        using var scope = _factory.Services.CreateScope();
        var feedIngestService = scope.ServiceProvider.GetRequiredService<IFeedIngestService>();
        
        await feedIngestService.IngestFeedsAsync(CancellationToken.None);

        // Assert - No exception should be thrown, and database should be empty
        var dbContext = scope.ServiceProvider.GetRequiredService<ZeitungDbContext>();
        var articles = await dbContext.Articles.ToListAsync();
        
        Assert.That(articles, Is.Empty, "No articles should be saved when no feeds are configured");
    }

    [Test]
    public async Task FeedIngestService_ShouldBeRegisteredInDI()
    {
        // Arrange
        _factory = await new AspireWorkerFactory().BuildAsync();

        // Act
        using var scope = _factory.Services.CreateScope();
        var feedIngestService = scope.ServiceProvider.GetService<IFeedIngestService>();

        // Assert
        Assert.That(feedIngestService, Is.Not.Null, "IFeedIngestService should be registered in DI container");
        Assert.That(feedIngestService, Is.InstanceOf<FeedIngestService>(), "Should resolve to FeedIngestService implementation");
    }

    [Test]
    public async Task AspireWorkerFactory_ShouldConfigureAllRequiredServices()
    {
        // Arrange & Act
        _factory = await new AspireWorkerFactory().BuildAsync();

        // Assert - Verify all critical services are registered
        using var scope = _factory.Services.CreateScope();
        
        Assert.That(scope.ServiceProvider.GetService<IFeedIngestService>(), Is.Not.Null, "IFeedIngestService should be registered");
        Assert.That(scope.ServiceProvider.GetService<IFeedParserFactory>(), Is.Not.Null, "IFeedParserFactory should be registered");
        Assert.That(scope.ServiceProvider.GetService<IArticleRepository>(), Is.Not.Null, "IArticleRepository should be registered");
        Assert.That(scope.ServiceProvider.GetService<ITagRepository>(), Is.Not.Null, "ITagRepository should be registered");
        Assert.That(scope.ServiceProvider.GetService<IElasticsearchService>(), Is.Not.Null, "IElasticsearchService should be registered");
        Assert.That(scope.ServiceProvider.GetService<ITaggingStrategy>(), Is.Not.Null, "ITaggingStrategy should be registered");
        Assert.That(scope.ServiceProvider.GetService<ZeitungDbContext>(), Is.Not.Null, "ZeitungDbContext should be registered");
    }
}
