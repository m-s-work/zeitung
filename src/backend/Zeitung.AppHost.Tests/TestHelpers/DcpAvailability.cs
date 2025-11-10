using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Zeitung.AppHost.Tests.TestHelpers;

/// <summary>
/// Helper class to check if DCP (Distributed Control Plane) is available for integration tests.
/// </summary>
public static class DcpAvailability
{
    private static bool? _isAvailable;
    private static string? _failureReason;
    private static readonly object _lock = new object();

    /// <summary>
    /// Checks if DCP is available for running integration tests.
    /// </summary>
    public static async Task<(bool IsAvailable, string? FailureReason)> CheckAsync()
    {
        lock (_lock)
        {
            if (_isAvailable.HasValue)
            {
                return (_isAvailable.Value, _failureReason);
            }
        }

        try
        {
            // Try to create a minimal AppHost to test DCP availability
            // Use a shorter timeout to fail fast
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.Zeitung_AppHost>();

            // Configure minimal logging to avoid spam
            builder.Services.AddLogging(logging => logging
                .SetMinimumLevel(LogLevel.Error));

            // Try to build - this will fail if DCP can't start
            // We don't actually need to start it, just build to verify DCP is available
            await using var app = await builder.BuildAsync(cts.Token);
            
            // If we got here, DCP is working
            lock (_lock)
            {
                _isAvailable = true;
                _failureReason = null;
            }
            
            return (true, null);
        }
        catch (OperationCanceledException)
        {
            lock (_lock)
            {
                _isAvailable = false;
                _failureReason = "DCP availability check timed out after 30 seconds";
            }
            return (false, _failureReason!);
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                _isAvailable = false;
                _failureReason = $"DCP not available: {ex.GetType().Name} - {ex.Message.Split('\n')[0]}";
            }
            return (false, _failureReason!);
        }
    }

    /// <summary>
    /// Resets the cached availability check result.
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _isAvailable = null;
            _failureReason = null;
        }
    }
}
