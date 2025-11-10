using Zeitung.Worker;
using Zeitung.Worker.Services;
using Zeitung.Worker.Strategies;

var builder = Host.CreateApplicationBuilder(args);

// Add service defaults
builder.AddServiceDefaults();

// Register HTTP client factory
builder.Services.AddHttpClient();

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

// Register worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
