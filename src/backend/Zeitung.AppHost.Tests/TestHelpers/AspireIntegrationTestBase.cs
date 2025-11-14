using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Zeitung.AppHost.Tests.TestHelpers;

/// <summary>
/// Base class for Aspire integration tests with common setup and teardown.
/// Manages the lifecycle of the DistributedApplication.
/// </summary>
public abstract class AspireIntegrationTestBase
{
    protected IDistributedApplicationTestingBuilder? Builder { get; private set; }
    protected DistributedApplication? App { get; private set; }

    /// <summary>
    /// Sets up the Aspire application once for all tests in the fixture.
    /// Override to customize HTTP client configuration.
    /// </summary>
    [OneTimeSetUp, CancelAfter(30_000)]
    public async Task OneTimeSetUpAsync(CancellationToken cancellationToken)
    {
        // Configure Aspire to run in CI environment to use appsettings.ci.json
        // This skips frontend startup and external RSS feed health checks during tests
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Zeitung_AppHost>(
            args: [],
            configureBuilder: (options, settings) =>
            {
                // Set environment to 'ci' to load appsettings.ci.json
                settings.EnvironmentName = "ci";
            },
            cancellationToken: cancellationToken
            );

        // Allow derived classes to configure the builder
        ConfigureBuilder(appHost);

        Builder = appHost;
        App = await appHost.BuildAsync(cancellationToken);
        await App.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Override to configure the builder with custom settings (e.g., HTTP client defaults).
    /// </summary>
    protected virtual void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        // Default implementation does nothing
        // Derived classes can override to add custom configuration
    }

    /// <summary>
    /// Tears down the Aspire application after all tests complete.
    /// </summary>
    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        if (App != null)
        {
            try
            {
                await App.StopAsync();
            }
            catch
            {
                // Ignore stop failures during teardown
            }

            try
            {
                await App.DisposeAsync();
            }
            catch
            {
                // Ignore dispose failures during teardown
            }

            App = null;
            Builder = null;
        }
    }
}
