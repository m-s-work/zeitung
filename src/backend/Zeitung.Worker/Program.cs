using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Zeitung.Worker;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;
using Zeitung.Worker.Strategies;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Add PostgreSQL with Aspire
builder.AddNpgsqlDbContext<ZeitungDbContext>("zeitungdb");

// Add Elasticsearch
var elasticsearchConnectionString = builder.Configuration.GetConnectionString("elasticsearch") ?? "http://localhost:9200";
builder.Services.AddSingleton(sp =>
{
    var settings = new ElasticsearchClientSettings(new Uri(elasticsearchConnectionString))
        .DefaultIndex("articles");
    return new ElasticsearchClient(settings);
});

// Register HTTP client factory
builder.Services.AddHttpClient();

// Register repositories and services
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ITagRepository, PostgresTagRepository>();
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();
builder.Services.AddScoped<IRssFeedParser, RssFeedParser>();
builder.Services.AddScoped<IFeedIngestService, FeedIngestService>();

// Register tagging strategy based on configuration
var taggingStrategy = builder.Configuration["TaggingStrategy"] ?? "FeedBased";
switch (taggingStrategy)
{
    case "Mock":
        builder.Services.AddScoped<ITaggingStrategy, MockTaggingStrategy>();
        break;
    case "LLM":
        builder.Services.AddScoped<ITaggingStrategy, LlmTaggingStrategy>();
        break;
    case "FeedBased":
    default:
        builder.Services.AddScoped<ITaggingStrategy, FeedBasedTaggingStrategy>();
        break;
}

// Check if running in one-off mode (e.g., for K8s CronJob)
var runOnce = args.Contains("--run-once") || args.Contains("-o");

// Check if migrations should be run (e.g., for pre-deploy hooks in Helm or Argo)
var runMigrations = args.Contains("--migrate") || args.Contains("-m");

if (runOnce)
{
    // One-off execution mode
    var host = builder.Build();
    
    // Run migrations if requested
    if (runMigrations)
    {
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ZeitungDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
    
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Running in one-off mode");
    
    try
    {
        using var scope = host.Services.CreateScope();
        var feedIngestService = scope.ServiceProvider.GetRequiredService<IFeedIngestService>();
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
    
    // Run migrations if requested
    if (runMigrations)
    {
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ZeitungDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
    
    await host.RunAsync();
    return 0;
}
