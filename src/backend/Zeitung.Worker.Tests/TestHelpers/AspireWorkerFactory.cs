// Based on: https://github.com/dotnet/aspire/discussions/878#discussioncomment-14572639

using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore;
using Zeitung.Core.Models;
using Zeitung.Worker.Jobs;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;
using Zeitung.Worker.Strategies;

namespace Zeitung.Worker.Tests.TestHelpers;

/// <summary>
/// Factory for creating test instances of the Worker application.
/// Similar to WebApplicationFactory but for Host-based applications.
/// Allows testing the Worker with a full DI container while substituting dependencies for testing.
/// </summary>
public class AspireWorkerFactory : IAsyncDisposable
{
    private IHost? _host;
    
    public IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("Host not built yet. Call BuildAsync first.");

    /// <summary>
    /// Builds the Worker host with all dependencies configured.
    /// </summary>
    /// <param name="configureBuilder">Optional action to configure the host builder for test customization</param>
    public async Task<AspireWorkerFactory> BuildAsync(Action<IHostApplicationBuilder>? configureBuilder = null)
    {
        var args = Array.Empty<string>();
        var builder = Host.CreateApplicationBuilder(args);
        
        // Set environment to Testing - this will load appsettings.Testing.json
        builder.Environment.EnvironmentName = "Testing";
        
        // Add service defaults (simplified - no actual Aspire orchestration needed in tests)
        // builder.AddServiceDefaults(); // Skip this in tests as it requires Aspire infrastructure
        
        // Add in-memory database for testing instead of real PostgreSQL
        var testDbName = $"WorkerIntegrationTest_{Guid.NewGuid():N}";
        builder.Services.AddDbContext<ZeitungDbContext>(options =>
        {
            options.UseInMemoryDatabase(testDbName);
        });
        
        // Add mock Elasticsearch
        builder.Services.AddSingleton<ElasticsearchClient>(sp =>
        {
            var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("articles");
            return new ElasticsearchClient(settings);
        });
        
        // Register HTTP client factory
        builder.Services.AddHttpClient();
        
        // Configure RSS feed options from test configuration
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
        
        // Register tagging strategy based on configuration (default to Mock for tests)
        var taggingStrategy = builder.Configuration["TaggingStrategy"] ?? "Mock";
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
        
        // NOTE: We do NOT add RssFeedIngestWorker as a hosted service in tests
        // because we want to control when feed ingestion happens
        
        // Allow test-specific configuration overrides
        configureBuilder?.Invoke(builder);
        
        // Build the host
        _host = builder.Build();
        
        // Initialize the database
        using (var scope = _host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ZeitungDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }
        
        return this;
    }

    public async ValueTask DisposeAsync()
    {
        if (_host != null)
        {
            try
            {
                await _host.StopAsync();
            }
            catch
            {
                // Ignore stop failures during teardown
            }

            _host.Dispose();
            _host = null;
        }
        GC.SuppressFinalize(this);
    }
}

