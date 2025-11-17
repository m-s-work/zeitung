using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Projects;

namespace Zeitung.AppHost.Tests.Harness;

/// <summary>
/// Base class for Aspire integration tests with common setup and teardown.
/// Manages the lifecycle of the DistributedApplication.
/// </summary>
public abstract class AspireIntegrationTestBase<TProgram>
    where TProgram : class
{
    //protected IDistributedApplicationTestingBuilder? Builder { get; private set; }
    protected DistributedApplication? App { get; private set; }

    public AspireWebApplicationFactory<TProgram, Zeitung_AppHost>? Factory { get; set; }

    /// <summary>
    /// Sets up the Aspire application once for all tests in the fixture.
    /// Override to customize HTTP client configuration.
    /// </summary>
    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        // create a token that cancels after 30 seconds
        CancellationToken cancellationToken = new CancellationTokenSource(30_000).Token;

        /*
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
        */
        Factory = new Zeitung.AppHost.Tests.Harness.AspireWebApplicationFactory<TProgram, Projects.Zeitung_AppHost>()
        {
            //ApiResourceName = "Zeitung.Api",
            ApiResourceName = "api",
            Ephemeral = true,
            Resources = [],
        };
        App = await Factory.InitializeAsync();
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
        //if (App != null)
        //{
        //    try
        //    {
        //        await App.StopAsync();
        //    }
        //    catch
        //    {
        //        // Ignore stop failures during teardown
        //    }
        //
        //    try
        //    {
        //        await App.DisposeAsync();
        //    }
        //    catch
        //    {
        //        // Ignore dispose failures during teardown
        //    }
        //
        //    App = null;
        //    Builder = null;
        //}

        if (Factory != null)
            await Factory.DisposeAsync();
    }
}
