using Zeitung.Worker.Services;

namespace Zeitung.Worker;

public class RssFeedIngestWorker : BackgroundService
{
    private readonly ILogger<RssFeedIngestWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public RssFeedIngestWorker(
        ILogger<RssFeedIngestWorker> logger, 
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Check if running in CI mode
        var isCI = _configuration.GetValue<bool>("CI");
        if (isCI)
        {
            _logger.LogInformation("Running in CI mode - Worker will not perform feed ingestion");
            // In CI mode, just stay alive but don't do anything
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        // Get ingestion interval from configuration (default: 5 minutes)
        var intervalMinutes = _configuration.GetValue<int>("IngestIntervalMinutes", 5);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        // Check if RSS feeds are configured
        var rssFeeds = _configuration.GetSection("RssFeeds").GetChildren();
        if (!rssFeeds.Any())
        {
            _logger.LogWarning("No RSS feeds configured. Worker will not perform feed ingestion.");
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        _logger.LogInformation("RSS Feed Worker started. Ingestion interval: {Interval} minutes", intervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var feedIngestService = scope.ServiceProvider.GetRequiredService<IFeedIngestService>();
                await feedIngestService.IngestFeedsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during feed ingestion");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
