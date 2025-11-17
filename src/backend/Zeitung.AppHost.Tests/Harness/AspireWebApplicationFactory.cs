// taken from https://github.com/dotnet/aspire/discussions/878#discussioncomment-14572639

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Zeitung.AppHost.Tests.Harness;

/// <summary>
/// A custom WebApplicationFactory that bootstraps an Aspire AppHost for integration tests.
/// It starts all required infrastructure/resources declared in the AppHost, removes the API-under-test from the
/// Aspire host, and then injects the API's configuration/environment into the test server.
/// </summary>
/// <typeparam name="TEntryPoint">Program class of the API under test (the SUT).</typeparam>
/// <typeparam name="TAppHost">The Aspire AppHost Program type (e.g., Projects.Pep_Host).</typeparam>
public class AspireWebApplicationFactory<TEntryPoint, TAppHost> : WebApplicationFactory<TEntryPoint>, IDisposable
    where TEntryPoint : class
    where TAppHost : class
{
    private DistributedApplication? _app;

    /// <summary>
    /// Name of the resource in the AppHost that corresponds to the API under test. If not provided, the
    /// factory will try to infer the name from the entrypoint assembly name.
    /// </summary>
    public string? ApiResourceName { get; init; }

    /// <summary>
    /// Optional callback invoked after the AppHost is built but before it is started. You can tweak
    /// resources (e.g., remove optional dashboards) or adjust configuration.
    /// </summary>
    public Action<IDistributedApplicationTestingBuilder>? ConfigureAppHost { get; init; }

    /// <summary>
    /// Optional callback to modify the DI container of the SUT.
    /// </summary>
    public Action<IServiceCollection>? ConfigureServices { get; init; }

    /// <summary>
    /// Optional callback invoked after the AppHost is built and started. You can implement readiness waits here.
    /// </summary>
    public Func<IServiceProvider, CancellationToken, Task>? AfterAppHostStartedAsync { get; init; }

    /// <summary>
    /// Resources the AppHost should include.
    /// if set to null no resource filtering will be applied.
    /// if set to empty all resources will be removed except the API-under-test.
    /// </summary>
    public List<string> Resources { get; init; } = [];

    /// <summary>
    /// Determines whether the container lifetimes should be removed from all resources in the app host, if false, this will allow persistence between test runs (useful while adding new tests for speed).
    /// </summary>
    public bool Ephemeral { get; init; } = true;

    private Dictionary<string, string?> _hostConfig = [];

    public async Task<DistributedApplication> InitializeAsync()
    {
        var testingBuilder = await DistributedApplicationTestingBuilder.CreateAsync<TAppHost>();

        var apiResource = ResolveApiResource(testingBuilder.Resources.ToList());
        if (apiResource is null)
            throw new InvalidOperationException(
                "Could not resolve the API-under-test resource from the AppHost. Provide ApiResourceName or ApiResourceSelector.");


        //if (Resources is not null)
        //{
            int added;
            do
            {
                var annotations = testingBuilder.Resources.Where(r =>
                        r.Annotations.OfType<ResourceRelationshipAnnotation>().Any(p =>
                            Resources.Contains(p.Resource.Name) && p.Type == "Parent" &&
                            !Resources.Contains(p.Resource.Name)))
                    .Select(r => r.Name);
                var parents = testingBuilder.Resources.Where(r => r is IResourceWithParent && !Resources.Contains(r.Name))
                    .Select(r => r.Name);

                List<string> adds = [.. annotations, .. parents];
                Resources.AddRange(adds);

                added = adds.Count;
            } while (added > 0);

            foreach (var resource in testingBuilder.Resources.Where(r => !Resources.Distinct().Contains(r.Name)).ToArray())
                testingBuilder.Resources.Remove(resource);
        //}
        

        if (Ephemeral)
        {
            foreach (var resource in testingBuilder.Resources)
            {
                var lifetime = resource.Annotations.OfType<ContainerLifetimeAnnotation>()?.FirstOrDefault();
                if (lifetime != null)
                    resource.Annotations.Remove(lifetime);
            }
        }

        ConfigureAppHost?.Invoke(testingBuilder);

        _app = await testingBuilder.BuildAsync();

        await Task.WhenAll(testingBuilder.Resources.Select(r => _app.ResourceNotifications.WaitForResourceHealthyAsync(r.Name)));

        await _app.StartAsync();

        if (AfterAppHostStartedAsync is not null)
            await AfterAppHostStartedAsync.Invoke(_app.Services, CancellationToken.None);

        _hostConfig = await ResolveConfigurationFromResourceAsync(apiResource, _app.Services);


        return _app;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        foreach (var (key, value) in _hostConfig)
        {
            if (value is null) continue;
            builder.UseSetting(key, value);
        }

        builder.ConfigureServices(s => ConfigureServices?.Invoke(s));
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> configured to communicate with the specified resource.
    /// </summary>
    /// <param name="resource">The name of the resource.</param>
    /// <param name="endpoint">The endpoint on the resource to communicate with.</param>
    /// <returns>The <see cref="HttpClient"/>.</returns>
    protected HttpClient CreateResourceClient(string resource, string endpoint = "https") => _app!.CreateHttpClient(resource, endpoint);

    private IResource? ResolveApiResource(IReadOnlyCollection<IResource> resources)
    {
        if (!string.IsNullOrWhiteSpace(ApiResourceName))
            return resources.FirstOrDefault(r => string.Equals(r.Name, ApiResourceName, StringComparison.OrdinalIgnoreCase));

        var inferred = typeof(TEntryPoint).Assembly.GetName().Name;

        return !string.IsNullOrWhiteSpace(inferred)
            ? resources.FirstOrDefault(r => string.Equals(r.Name, inferred, StringComparison.OrdinalIgnoreCase))
            : null;
    }

    /// <summary>
    /// Loads environment variables exposed by the resource and translates double-underscore keys to configuration keys.
    /// </summary>
    private static async Task<Dictionary<string, string?>> ResolveConfigurationFromResourceAsync(IResource resource, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var config = new Dictionary<string, string?>();

        if (resource is not IResourceWithEnvironment resourceWithEnvironment || !resourceWithEnvironment.TryGetEnvironmentVariables(out var annotations))
            return config;

        var options = new DistributedApplicationExecutionContextOptions(DistributedApplicationOperation.Run)
        {
            ServiceProvider = serviceProvider
        };
        var execContext = new DistributedApplicationExecutionContext(options);
        var context = new EnvironmentCallbackContext(execContext, cancellationToken: cancellationToken);

        foreach (var annotation in annotations)
            await annotation.Callback(context);

        foreach (var (key, value) in context.EnvironmentVariables)
        {
            if (resource is ProjectResource && key == "ASPNETCORE_URLS") continue;

            string? configValue;
            switch (value)
            {
                case string s:
                    configValue = s;
                    break;
                case IValueProvider v:
                    try
                    {
                        configValue = await v.GetValueAsync(cancellationToken);
                    }
                    catch
                    {
                        configValue = null;
                    }
                    break;
                case null:
                    configValue = null;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported environment value type: {value.GetType()}");
            }

            if (configValue is not null)
                config[key.Replace("__", ":")] = configValue;
        }

        return config;
    }

    public override async ValueTask DisposeAsync()
    {
        if (_app is not null)
            await _app.DisposeAsync();
        await base.DisposeAsync();
    }
}
