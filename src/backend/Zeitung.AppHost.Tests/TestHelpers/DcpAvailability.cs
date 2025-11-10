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

    /// <summary>
    /// Checks if DCP is available for running integration tests.
    /// </summary>
    public static async Task<(bool IsAvailable, string? FailureReason)> CheckAsync()
    {
        if (_isAvailable.HasValue)
        {
            return (_isAvailable.Value, _failureReason);
        }

        try
        {
            // Try to create a minimal AppHost to test DCP availability
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.Zeitung_AppHost>();

            // Configure minimal logging to avoid spam
            builder.Services.AddLogging(logging => logging
                .SetMinimumLevel(LogLevel.Error));

            // Try to build - this will fail if DCP can't start
            await using var app = await builder.BuildAsync();
            
            // If we got here, DCP is working
            _isAvailable = true;
            _failureReason = null;
            
            return (true, null);
        }
        catch (Exception ex)
        {
            _isAvailable = false;
            _failureReason = $"DCP not available: {ex.GetType().Name} - {ex.Message}";
            return (false, _failureReason);
        }
    }

    /// <summary>
    /// Resets the cached availability check result.
    /// </summary>
    public static void Reset()
    {
        _isAvailable = null;
        _failureReason = null;
    }
}
