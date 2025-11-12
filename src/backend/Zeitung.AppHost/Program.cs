using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = DistributedApplication.CreateBuilder(args);

// Check if running in CI environment
var isCI = builder.Configuration.GetValue<bool>("CI");

// Check if frontend should be included (disabled in CI by default)
var includeFrontend = builder.Configuration.GetValue<bool>("IncludeFrontend", !isCI);

// Add external services (RSS feeds and OpenRouter)
// These are used by the worker to know when external dependencies are available
var bbcNews = builder.AddExternalService("bbc-news", "http://feeds.bbci.co.uk")
    .WithHttpHealthCheck(path: "/news/rss.xml");

var heiseOnline = builder.AddExternalService("heise-online", "https://www.heise.de")
    .WithHttpHealthCheck(path: "/rss/heise-atom.xml");

var golem = builder.AddExternalService("golem", "https://rss.golem.de")
    .WithHttpHealthCheck(path: "/rss.php?feed=RSS2.0");

var orf = builder.AddExternalService("orf", "https://rss.orf.at")
    .WithHttpHealthCheck(path: "/news.xml");

var openRouter = builder.AddExternalService("openrouter", "https://openrouter.ai")
    .WithHttpHealthCheck(path: "/api/v1/models");

// Add PostgreSQL with lifecycle event hook
var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .OnBeforeResourceStarted((resource, evt, cancellationToken) =>
    {
        var logger = evt.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("PostgreSQL container starting...");
        return Task.CompletedTask;
    });

var postgresdb = postgres.AddDatabase("zeitungdb");

// Add Redis with lifecycle event hook
var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent)
    .OnBeforeResourceStarted((resource, evt, cancellationToken) =>
    {
        var logger = evt.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Redis container starting...");
        return Task.CompletedTask;
    });

// Add Elasticsearch with lifecycle event hook
var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithLifetime(ContainerLifetime.Persistent)
    .OnBeforeResourceStarted((resource, evt, cancellationToken) =>
    {
        var logger = evt.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Elasticsearch container starting...");
        return Task.CompletedTask;
    });

// do ef core migrations
var workerMigrator = builder.AddProject<Projects.Zeitung_Worker>("migrator")
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    .WithArgs("--migrate");


// Add the API service
var api = builder.AddProject<Projects.Zeitung_Api>("api")
    .WithReference(postgresdb)
    .WithReference(redis)
    .WithReference(elasticsearch)

    .WaitFor(postgresdb)
    .WaitFor(redis)
    .WaitFor(elasticsearch)

    .WaitForCompletion(workerMigrator);

// Add the Worker service for RSS feed ingestion
// In CI mode, worker will use Mock strategy and won't need external feeds
var worker = builder.AddProject<Projects.Zeitung_Worker>("worker")
    .WithReference(postgresdb)
    .WithReference(redis)
    .WithReference(elasticsearch)

    .WaitFor(postgresdb)
    .WaitFor(redis)
    .WaitFor(elasticsearch)

    .WaitForCompletion(workerMigrator);

// Add references to external services for health monitoring
// Worker will check if these are available before attempting to fetch feeds
if (!isCI)
{
    worker = worker
        .WithReference(bbcNews)
        .WithReference(heiseOnline)
        .WithReference(golem)
        .WithReference(orf)
        .WithReference(openRouter);
}

// Add the frontend npm app if not in CI mode
// In CI/CD tests, we skip the frontend to avoid npm install overhead in C# integration tests
if (includeFrontend)
{
    // Get the path to the frontend directory (relative to AppHost or absolute)
    var frontendPath = Path.Combine("..", "..", "frontend");
    var frontendFullPath = Path.GetFullPath(frontendPath);


    // Run npm ci to install dependencies before starting the frontend
    var npmInstall = builder.AddExecutable("npm-install", "npm", frontendPath, "ci");
    
    var frontend = builder.AddNpmApp("frontend", frontendPath, "dev")
        .WithHttpEndpoint(env: "PORT")
        .WithExternalHttpEndpoints()
        .WithReference(api)
        .WaitFor(api)
        .WaitForCompletion(npmInstall); // Wait for npm ci to complete before starting
}

builder.Build().Run();
