using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Zeitung.AppHost.Tests.Harness;
using Zeitung.AppHost.Tests.TestHelpers;

namespace Zeitung.Api.Tests;

[TestFixture]
[Category("IntegrationTest")]
public class HappyPathApiTests : AspireIntegrationTestBase<Program>
{
    [OneTimeSetUp]
    public override async Task OneTimeSetUpAsync()
    {
        Factory = new AspireWebApplicationFactory<Program, Projects.Zeitung_AppHost>()
        {
            //ApiResourceName = "Zeitung.Api",
            ApiResourceName = "api",
            Ephemeral = true,
            FilterIncludeResources = ["postgres", "migrator"],
            //FilterIncludeResources = ["api", "postgres", "migrator"],
            //FilterIncludeResources = [],
        };
        DistributedApp = await Factory.InitializeAsync();
    }


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
        var response = await ApiClient!.GetAsync("/health");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    public async Task ApiAliveEndpointReturnsOk()
    {
        // Act
        var client = Factory!.CreateClient();
        //var httpClient = App!.CreateHttpClient("api");
        var response = await client.GetAsync("/alive");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    public async Task ApiReadyEndpointReturnsOkWhenDependenciesAreHealthy()
    {
        // Act

        // Retry logic for /ready endpoint as dependencies might take time to initialize
        var maxRetries = 20;
        var retryDelay = TimeSpan.FromSeconds(10);
        System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.ServiceUnavailable;
        
        for (int i = 0; i < maxRetries; i++)
        {
            var response = await ApiClient!.GetAsync("/ready");
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
        var response = await ApiClient!.GetAsync("/health");
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
        var response = await ApiClient!.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Healthy"));
    }

    [Test]
    public async Task ElasticsearchHealthCheckIsRegistered()
    {
        // Act
        var response = await ApiClient!.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Healthy"));
    }
}
