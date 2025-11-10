using Zeitung.Worker.Models;
using Zeitung.Worker.Strategies;

namespace Zeitung.Worker.Services;

public interface IFeedIngestService
{
    Task IngestFeedsAsync(CancellationToken cancellationToken = default);
}

public class FeedIngestService : IFeedIngestService
{
    private readonly IRssFeedParser _parser;
    private readonly ITaggingStrategy _taggingStrategy;
    private readonly ILogger<FeedIngestService> _logger;
    private readonly List<RssFeed> _feeds;

    public FeedIngestService(
        IRssFeedParser parser,
        ITaggingStrategy taggingStrategy,
        ILogger<FeedIngestService> logger,
        IConfiguration configuration)
    {
        _parser = parser;
        _taggingStrategy = taggingStrategy;
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

                    // TODO: Store article in database
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
