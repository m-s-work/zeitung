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

    public IntegrationTestBase()
    {
        // Set environment variable BEFORE creating the factory
        Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
        
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

                    // Add in-memory database for testing
                    services.AddDbContext<ZeitungDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
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

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
