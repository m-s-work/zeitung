using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zeitung.Core.Models;

namespace Zeitung.Api.Tests;

/// <summary>
/// Base class for API integration tests with in-memory database
/// </summary>
public class IntegrationTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    private readonly string _databaseName;

    public IntegrationTestBase()
    {
        // Set environment variable BEFORE creating the factory
        Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
        
        // Use a unique database name for this test fixture instance
        _databaseName = $"TestDb_{Guid.NewGuid()}";
        
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                builder.ConfigureServices(services =>
                {
                    // Remove all existing DbContext registrations to avoid conflicts
                    var descriptorsToRemove = services.Where(
                        d => d.ServiceType == typeof(DbContextOptions<ZeitungDbContext>) ||
                             d.ServiceType == typeof(ZeitungDbContext) ||
                             d.ServiceType == typeof(DbContextOptions)).ToList();
                    
                    foreach (var descriptor in descriptorsToRemove)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing with shared database name
                    services.AddDbContext<ZeitungDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(_databaseName);
                    });
                });
            });

        Client = Factory.CreateClient();
        
        // Ensure database is created
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ZeitungDbContext>();
        db.Database.EnsureCreated();
    }

    protected ZeitungDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ZeitungDbContext>();
    }
    
    protected void ClearDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ZeitungDbContext>();
        
        // Remove all data from all tables
        db.UserFeeds.RemoveRange(db.UserFeeds);
        db.UserTags.RemoveRange(db.UserTags);
        db.Votes.RemoveRange(db.Votes);
        db.RefreshTokens.RemoveRange(db.RefreshTokens);
        db.MagicLinks.RemoveRange(db.MagicLinks);
        db.ArticleTags.RemoveRange(db.ArticleTags);
        db.TagCoOccurrences.RemoveRange(db.TagCoOccurrences);
        db.Articles.RemoveRange(db.Articles);
        db.Tags.RemoveRange(db.Tags);
        db.Feeds.RemoveRange(db.Feeds);
        db.Users.RemoveRange(db.Users);
        
        db.SaveChanges();
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
