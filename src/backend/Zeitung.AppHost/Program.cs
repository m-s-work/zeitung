var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL
var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

var postgresdb = postgres.AddDatabase("zeitungdb");

// Add Redis
var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

// Add Elasticsearch
var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithLifetime(ContainerLifetime.Persistent);

// Add the API service
var api = builder.AddProject<Projects.Zeitung_Api>("api")
    .WithReference(postgresdb)
    .WithReference(redis)
    .WithReference(elasticsearch);

builder.Build().Run();
