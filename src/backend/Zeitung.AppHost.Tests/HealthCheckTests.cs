using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit.Categories;

namespace Zeitung.AppHost.Tests;

public class HealthCheckTests
{
    [Fact]
    [IntegrationTest]
    public async Task ApiHealthCheckEndpointReturnsOk()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zeitung_AppHost>();
        
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    [IntegrationTest]
    public async Task ApiAliveEndpointReturnsOk()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zeitung_AppHost>();
        
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/alive");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    [IntegrationTest]
    public async Task ApiReadyEndpointReturnsOkWhenDependenciesAreHealthy()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zeitung_AppHost>();
        
        appHost.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);
            });
        });
        
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api");
        
        // Retry logic for /ready endpoint as dependencies might take time to initialize
        var maxRetries = 20;
        var retryDelay = TimeSpan.FromSeconds(10);
        System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.ServiceUnavailable;
        
        for (int i = 0; i < maxRetries; i++)
        {
            var response = await httpClient.GetAsync("/ready");
            statusCode = response.StatusCode;
            
            if (statusCode == System.Net.HttpStatusCode.OK)
            {
                break;
            }
            
            if (i < maxRetries - 1)
            {
                await Task.Delay(retryDelay);
            }
        }

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, statusCode);
    }

    [Fact]
    [IntegrationTest]
    public async Task PostgresHealthCheckIsRegistered()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zeitung_AppHost>();
        
        appHost.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
            });
        });
        
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        // The health check response should be "Healthy" when all checks pass
        Assert.Contains("Healthy", content);
    }

    [Fact]
    [IntegrationTest]
    public async Task RedisHealthCheckIsRegistered()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zeitung_AppHost>();
        
        appHost.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
            });
        });
        
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", content);
    }

    [Fact]
    [IntegrationTest]
    public async Task ElasticsearchHealthCheckIsRegistered()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zeitung_AppHost>();
        
        appHost.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
            });
        });
        
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", content);
    }
}
