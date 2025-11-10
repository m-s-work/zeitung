using Zeitung.Worker.Services;

namespace Zeitung.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IFeedIngestService _feedIngestService;
    private readonly IConfiguration _configuration;

    public Worker(
        ILogger<Worker> logger, 
        IFeedIngestService feedIngestService,
        IConfiguration configuration)
    {
        _logger = logger;
        _feedIngestService = feedIngestService;
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
                await _feedIngestService.IngestFeedsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during feed ingestion");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
