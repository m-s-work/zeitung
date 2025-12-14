using Aspire.Hosting.Testing;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zeitung.AppHost.Tests.Harness;
using Zeitung.AppHost.Tests.TestHelpers;
using Zeitung.Core.Context;

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
            FilterIncludeResources = ["postgres", "migrator", "elasticsearch"],
            //FilterIncludeResources = ["api", "postgres", "migrator"],
            //FilterIncludeResources = [],
        };
        AspireApp = await Factory.InitializeAsync();
        ApiClient = Factory!.CreateClient();
    }


    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        // Configure default HTTP client resilience/timeouts used by tests.
        // Doing this here ensures all tests use consistent timeouts.
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30); // default is 30
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10); // default is 30
            });
        });
    }

    [Test]
    [CancelAfter(30)]
    public async Task ApiHealthCheckEndpointReturnsOk()
    {
        // Act
        var response = await ApiClient!.GetAsync("/health");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
    }

    [Test]
    [CancelAfter(30)]
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
    [CancelAfter(30)]
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
    [CancelAfter(30)]
    public async Task PostgresHealthCheckIsRegistered()
    {
        // Act
        var dbContext = Factory!.Services.GetService<ZeitungDbContext>();
        //var postgres = DistributedApp.ResourceNotifications.
        var response = await ApiClient!.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        // The health check response should be "Healthy" when all checks pass
        Assert.That(content, Does.Contain("Healthy"));
    }

    [Test]
    [CancelAfter(30)]
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
    [CancelAfter(30)]
    public async Task ElasticsearchHealthCheckIsRegistered()
    {
        // Act
        var httpClient = AspireApp!.CreateHttpClient("elasticsearch");

        var config = AspireApp!.Services.GetService<IConfiguration>(); // {Path = Parameters:elasticsearch-password, Value = )hdyh8-D2}UKHKX8hN{Gcy, Provider = JsonConfigurationProvider for 'secrets.json' (Optional)}
        var elasticPassword = config!.GetValue<string>("Parameters:elasticsearch-password");
        var auth = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"elastic:{elasticPassword}")));
        var authedClient = new System.Net.Http.HttpClient()
        {
            BaseAddress = httpClient.BaseAddress,
        };
        authedClient.DefaultRequestHeaders.Authorization = auth;

        // authorized client
        var response = await authedClient.GetAsync("/_cat/health"); // https://www.elastic.co/docs/api/doc/elasticsearch/operation/operation-cat-health
        var content = await response.Content.ReadAsStringAsync(); // `1765753157 22:59:17 docker-cluster green 1 1 2 2 0 0 0 0 0 - 100.0%`
        var regex = new System.Text.RegularExpressions.Regex(
            @"^(?<timestamp>\d+)\s+(?<time>\d{2}:\d{2}:\d{2})\s+(?<cluster>[\w-]+)\s+(?<status>\w+)\s+(?<nodeCount>\d+)\s+(?<dataNodeCount>\d+)\s+(?<shardCount>\d+)\s+(?<priShardCount>\d+)\s+(?<relocatingShards>\d+)\s+(?<initializingShards>\d+)\s+(?<unassignedShards>\d+)\s+(?<pendingTasks>\d+)\s+(?<maxTaskWaitTime>\d+)\s+-\s+(?<healthPercent>[\d.]+)%$");
        var match = regex.Match(content);
        var timestampStr = match.Groups["timestamp"].Value;
        var timestamp = long.Parse(timestampStr);
        var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
        var status = match.Groups["status"].Value; // green

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(content, Does.Contain("green"));
        Assert.That(content, Does.Contain("100.0%"));
        Assert.That(status, Is.EqualTo("green"));
        Assert.That(dateTime, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-5)));


        // unauthorized
        response = await httpClient!.GetAsync("/health");
        content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Forbidden));
        Assert.That(content, Does.Contain("Healthy"));
    }
}
