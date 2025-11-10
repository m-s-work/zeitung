using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = DistributedApplication.CreateBuilder(args);

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
var worker = builder.AddProject<Projects.Zeitung_Worker>("worker")
    .WithReference(postgresdb)
    .WithReference(redis)
    .WithReference(elasticsearch)

    .WaitFor(postgresdb)
    .WaitFor(redis)
    .WaitFor(elasticsearch)

    .WaitForCompletion(workerMigrator);

builder.Build().Run();
