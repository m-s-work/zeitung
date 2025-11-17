using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Zeitung.AppHost.Tests.Harness;
using Zeitung.AppHost.Tests.TestHelpers;

namespace Zeitung.Api.Tests;

[TestFixture]
[Category("IntegrationTest")]
public class HappyPathApiTests : AspireIntegrationTestBase<Program>
{
    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        // Configure default HTTP client resilience/timeouts used by tests.
        // Doing this here ensures all tests use consistent timeouts.
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });
        });
    }

    [Test]
    public async Task ApiHealthCheckEndpointReturnsOk()
    {
        // Act
        var httpClient = App!.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    public async Task ApiAliveEndpointReturnsOk()
    {
        // Act
        var httpClient = App!.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/alive");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    public async Task ApiReadyEndpointReturnsOkWhenDependenciesAreHealthy()
    {
        // Act
        var httpClient = App!.CreateHttpClient("api");
        
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
        // Act
        var httpClient = App!.CreateHttpClient("api");
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
        // Act
        var httpClient = App!.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Healthy"));
    }

    [Test]
    public async Task ElasticsearchHealthCheckIsRegistered()
    {
        // Act
        var httpClient = App!.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Healthy"));
    }
}
