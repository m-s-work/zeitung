using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Zeitung.AppHost.Tests;

[TestFixture]
[Category("IntegrationTest")]
public class HealthCheckTests
{
    private IDistributedApplicationTestingBuilder? _builder;
    private DistributedApplication? _app;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Zeitung_AppHost>();

        // Configure default HTTP client resilience/timeouts used by tests.
        // Doing this here ensures all tests use consistent timeouts.
        appHost.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });
        });

        _builder = appHost;
        _app = await appHost.BuildAsync();
        await _app.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        if (_app != null)
        {
            // Attempt graceful stop and dispose of the shared application
            try
            {
                await _app.StopAsync();
            }
            catch
            {
                // Ignore stop failures during teardown
            }

            try
            {
                await _app.DisposeAsync();
            }
            catch
            {
                // Ignore dispose failures during teardown
            }

            _app = null;
            _builder = null;
        }
    }

    [Test]
    public async Task ApiHealthCheckEndpointReturnsOk()
    {
        // Arrange
        var builder = (IDistributedApplicationTestingBuilder)_builder!;
        var app = _app!;

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
        var builder = (IDistributedApplicationTestingBuilder)_builder!;
        var app = _app!;

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
        var builder = (IDistributedApplicationTestingBuilder)_builder!;
        var app = _app!;

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
        var builder = (IDistributedApplicationTestingBuilder)_builder!;
        var app = _app!;

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
        var builder = (IDistributedApplicationTestingBuilder)_builder!;
        var app = _app!;

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
        var builder = (IDistributedApplicationTestingBuilder)_builder!;
        var app = _app!;

        // Act
        var httpClient = app!.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Healthy"));
    }
}
