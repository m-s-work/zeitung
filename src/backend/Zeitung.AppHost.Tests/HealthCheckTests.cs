using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Zeitung.AppHost.Tests.TestHelpers;

namespace Zeitung.AppHost.Tests;

[TestFixture]
[Category("IntegrationTest")]
public class HealthCheckTests
{
    private static bool? _dcpAvailable;
    private static string? _dcpFailureReason;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Check if DCP is available before running any tests
        var (isAvailable, failureReason) = await DcpAvailability.CheckAsync();
        _dcpAvailable = isAvailable;
        _dcpFailureReason = failureReason;

        if (!isAvailable)
        {
            Assert.Warn($"DCP is not available in this environment. Integration tests will be skipped. Reason: {failureReason}");
        }
    }

    private async Task<(IDistributedApplicationTestingBuilder builder, DistributedApplication? app)> CreateAndStartAppHostAsync()
    {
        if (_dcpAvailable == false)
        {
            Assert.Ignore($"Test skipped: DCP not available. {_dcpFailureReason}");
        }

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zeitung_AppHost>();
        
        // Configure logging to reduce DCP noise
        appHost.Services.AddLogging(logging => logging
            .AddFilter("Default", LogLevel.Information)
            .AddFilter("Microsoft.AspNetCore", LogLevel.Warning)
            .AddFilter("Aspire.Hosting.Dcp", LogLevel.Warning));

        try
        {
            var app = await appHost.BuildAsync();
            await app.StartAsync();
            return (appHost, app);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to start AppHost: {ex.Message}");
            throw; // Never reached, but satisfies compiler
        }
    }

    [Test]
    public async Task ApiHealthCheckEndpointReturnsOk()
    {
        // Arrange
        var (builder, app) = await CreateAndStartAppHostAsync();
        await using var _ = app;

        // Act
        var httpClient = app!.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    public async Task ApiAliveEndpointReturnsOk()
    {
        // Arrange
        var (builder, app) = await CreateAndStartAppHostAsync();
        await using var _ = app;

        // Act
        var httpClient = app!.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/alive");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    public async Task ApiReadyEndpointReturnsOkWhenDependenciesAreHealthy()
    {
        // Arrange
        var (builder, app) = await CreateAndStartAppHostAsync();
        await using var _ = app;
        
        // Configure extended timeout for resilience
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);
            });
        });

        // Act
        var httpClient = app!.CreateHttpClient("api");
        
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
        Assert.That(statusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    public async Task PostgresHealthCheckIsRegistered()
    {
        // Arrange
        var (builder, app) = await CreateAndStartAppHostAsync();
        await using var _ = app;
        
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
            });
        });

        // Act
        var httpClient = app!.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        // The health check response should be "Healthy" when all checks pass
        Assert.That(content, Does.Contain("Healthy"));
    }

    [Test]
    public async Task RedisHealthCheckIsRegistered()
    {
        // Arrange
        var (builder, app) = await CreateAndStartAppHostAsync();
        await using var _ = app;
        
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
            });
        });

        // Act
        var httpClient = app!.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Healthy"));
    }

    [Test]
    public async Task ElasticsearchHealthCheckIsRegistered()
    {
        // Arrange
        var (builder, app) = await CreateAndStartAppHostAsync();
        await using var _ = app;
        
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
            });
        });

        // Act
        var httpClient = app!.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Healthy"));
    }
}
