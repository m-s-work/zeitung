using Zeitung.Worker.Models;
using Zeitung.Core.Models;
using Zeitung.Worker.Strategies;
using ArticleDto = Zeitung.Worker.Models.Article;

namespace Zeitung.Worker.Services;

public interface IFeedIngestService
{
    Task IngestFeedsAsync(CancellationToken cancellationToken = default);
}

public class FeedIngestService : IFeedIngestService
{
    private readonly IRssFeedParser _parser;
    private readonly ITaggingStrategy _taggingStrategy;
    private readonly IArticleRepository _articleRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<FeedIngestService> _logger;
    private readonly List<RssFeed> _feeds;

    public FeedIngestService(
        IRssFeedParser parser,
        ITaggingStrategy taggingStrategy,
        IArticleRepository articleRepository,
        ITagRepository tagRepository,
        IElasticsearchService elasticsearchService,
        ILogger<FeedIngestService> logger,
        IConfiguration configuration)
    {
        _parser = parser;
        _taggingStrategy = taggingStrategy;
        _articleRepository = articleRepository;
        _tagRepository = tagRepository;
        _elasticsearchService = elasticsearchService;
        _logger = logger;
        
        // Load feeds from configuration
        _feeds = configuration.GetSection("RssFeeds").Get<List<RssFeed>>() ?? new List<RssFeed>();
    }

    public async Task IngestFeedsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting RSS feed ingestion for {Count} feeds", _feeds.Count);

        foreach (var feed in _feeds)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                _logger.LogInformation("Processing feed: {FeedName} ({FeedUrl})", feed.Name, feed.Url);

                var articles = await _parser.ParseFeedAsync(feed, cancellationToken);

                foreach (var article in articles)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Generate tags for the article
                    article.Tags = await _taggingStrategy.GenerateTagsAsync(article, cancellationToken);

                    _logger.LogInformation(
                        "Article: {Title} | Tags: {Tags}", 
                        article.Title, 
                        string.Join(", ", article.Tags));

                    // Store article in PostgreSQL
                    var articleEntity = await _articleRepository.SaveAsync(article, cancellationToken);

                    // Store tags and relationships
                    await _tagRepository.SaveArticleTagsAsync(articleEntity.Id, article.Tags, cancellationToken);

                    // Index article in Elasticsearch
                    await _elasticsearchService.IndexArticleAsync(article, articleEntity.Id, cancellationToken);
                }

                _logger.LogInformation("Completed processing feed: {FeedName}", feed.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing feed: {FeedName}", feed.Name);
            }
        }

        _logger.LogInformation("Completed RSS feed ingestion");
    }
}
