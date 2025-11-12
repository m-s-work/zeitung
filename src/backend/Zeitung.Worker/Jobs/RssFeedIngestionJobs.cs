using TickerQ.Utilities.Base;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Jobs;

/// <summary>
/// TickerQ jobs for RSS feed ingestion
/// </summary>
public class RssFeedIngestionJobs
{
    private readonly IFeedIngestService _feedIngestService;
    private readonly ILogger<RssFeedIngestionJobs> _logger;

    public RssFeedIngestionJobs(
        IFeedIngestService feedIngestService,
        ILogger<RssFeedIngestionJobs> logger)
    {
        _feedIngestService = feedIngestService;
        _logger = logger;
    }

    /// <summary>
    /// Main RSS feed ingestion job
    /// Scheduled via cron expression in configuration or executed on-demand
    /// </summary>
    [TickerFunction(functionName: "IngestRssFeeds")]
    public async Task IngestRssFeeds(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TickerQ job 'IngestRssFeeds' starting execution at {Time}", DateTime.UtcNow);
        
        try
        {
            await _feedIngestService.IngestFeedsAsync(cancellationToken);
            _logger.LogInformation("TickerQ job 'IngestRssFeeds' completed successfully at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TickerQ job 'IngestRssFeeds' failed");
            throw; // Re-throw to let TickerQ handle retries
        }
    }
}
