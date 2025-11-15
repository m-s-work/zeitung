using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore;
using Zeitung.Worker;
using Zeitung.Core.Models;
using Zeitung.Worker.Services;
using Zeitung.Worker.Strategies;
using Zeitung.Worker.Jobs;
using Zeitung.Worker.Models;

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

// Configure RSS feed options
builder.Services.Configure<RssFeedOptions>(options =>
{
    var feeds = builder.Configuration.GetSection(RssFeedOptions.SectionName).Get<List<RssFeed>>() ?? new();
    options.Feeds = feeds;
});

// Register repositories and services
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ITagRepository, PostgresTagRepository>();
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();

// Register feed parsers
builder.Services.AddScoped<IRdfFeedParser, RdfFeedParser>();
builder.Services.AddScoped<IRssFeedParser, RssFeedParser>();
builder.Services.AddScoped<IHtmlFeedParser, HtmlFeedParser>();

// Register all parsers for factory
builder.Services.AddScoped<IFeedParser, RssFeedParser>();
builder.Services.AddScoped<IFeedParser, HtmlFeedParser>();

// Register feed parser factory
builder.Services.AddScoped<IFeedParserFactory, FeedParserFactory>();

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

// Configure TickerQ for job scheduling
builder.Services.AddTickerQ(options =>
{
    options.SetMaxConcurrency(5);
});

// Register TickerQ job classes
builder.Services.AddScoped<RssFeedIngestionJobs>();

// Register the background worker service - Keep for backward compatibility
// This will be replaced by TickerQ in continuous mode but can co-exist
builder.Services.AddHostedService<RssFeedIngestWorker>();

// Check if running in one-off mode (e.g., for K8s CronJob)
var runOnce = args.Contains("--run-once") || args.Contains("-o");
// Check if migrations should be run (e.g., for pre-deploy hooks in Helm or Argo)
var runMigrations = args.Contains("--migrate") || args.Contains("-m");

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

// Run migrations if requested
if (runMigrations)
{
    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ZeitungDbContext>();
        await dbContext.Database.MigrateAsync();
        return 0;
    }
}

if (runOnce)
{
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
    // Continuous background service mode with TickerQ
    logger.LogInformation("Running in continuous mode with TickerQ scheduler");
    await host.RunAsync();
    return 0;
}
