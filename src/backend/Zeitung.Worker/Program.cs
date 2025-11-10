using Zeitung.Worker;
using Zeitung.Worker.Services;
using Zeitung.Worker.Strategies;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Register HTTP client factory
builder.Services.AddHttpClient();

// Register tag repository (in-memory for now, can be replaced with database implementation)
builder.Services.AddSingleton<ITagRepository, InMemoryTagRepository>();

// Register services
builder.Services.AddSingleton<IRssFeedParser, RssFeedParser>();
builder.Services.AddSingleton<IFeedIngestService, FeedIngestService>();

// Register tagging strategy based on configuration
var taggingStrategy = builder.Configuration["TaggingStrategy"] ?? "FeedBased";
switch (taggingStrategy)
{
    case "Mock":
        builder.Services.AddSingleton<ITaggingStrategy, MockTaggingStrategy>();
        break;
    case "LLM":
        builder.Services.AddSingleton<ITaggingStrategy, LlmTaggingStrategy>();
        break;
    case "FeedBased":
    default:
        builder.Services.AddSingleton<ITaggingStrategy, FeedBasedTaggingStrategy>();
        break;
}

// Check if running in one-off mode (e.g., for K8s CronJob)
var runOnce = args.Contains("--run-once") || args.Contains("-o");

if (runOnce)
{
    // One-off execution mode
    var host = builder.Build();
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    var feedIngestService = host.Services.GetRequiredService<IFeedIngestService>();
    
    logger.LogInformation("Running in one-off mode");
    
    try
    {
        await feedIngestService.IngestFeedsAsync(CancellationToken.None);
        logger.LogInformation("One-off feed ingestion completed successfully");
        return 0;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during one-off feed ingestion");
        return 1;
    }
}
else
{
    // Continuous background service mode
    builder.Services.AddHostedService<RssFeedIngestWorker>();
    
    var host = builder.Build();
    await host.RunAsync();
    return 0;
}
