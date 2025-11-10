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
        // Get ingestion interval from configuration (default: 5 minutes)
        var intervalMinutes = _configuration.GetValue<int>("IngestIntervalMinutes", 5);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

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
