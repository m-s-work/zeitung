using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zeitung.Core.Context;

namespace Zeitung.Api.Tests;

/// <summary>
/// Base class for API integration tests with in-memory database.
/// Uses unique test identifiers to enable parallel test execution.
/// </summary>
public class IntegrationTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly string TestId;
    private readonly string _databaseName;

    public IntegrationTestBase()
    {
        // Set environment variable BEFORE creating the factory
        Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
        
        // Generate unique test ID for this test instance to enable parallel execution
        TestId = Guid.NewGuid().ToString("N")[..8]; // Short 8-char ID
        
        // Use a shared database name for all tests (parallel-safe due to unique TestId in data)
        _databaseName = "TestDb_Shared";
        
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

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
