# Zeitung AppHost Integration Tests

## Overview

This project contains integration tests for the Zeitung AppHost using .NET Aspire's testing framework.

## Requirements

These tests require the .NET Aspire Developer Control Plane (DCP) to be available and functional. DCP is responsible for orchestrating containers and managing the lifecycle of Aspire resources during testing.

### Prerequisites

1. **.NET 9.0 SDK** or later
2. **Docker** installed and running
3. **.NET Aspire workload** installed (`dotnet workload install aspire`)
4. **DCP** - Automatically installed with Aspire workload

## Running Tests

### Locally

```bash
cd src/backend
dotnet test Zeitung.AppHost.Tests/Zeitung.AppHost.Tests.csproj --configuration Release
```

### In CI/CD

Integration tests may be skipped in CI/CD environments where DCP cannot establish its Kubernetes API endpoint. Tests will automatically detect DCP availability and mark themselves as ignored if DCP is not available.

## Test Categories

- **IntegrationTest**: Full end-to-end tests that start the entire AppHost with all dependencies (PostgreSQL, Redis, Elasticsearch)

## Known Issues

### DCP Connection Issues

If tests fail with messages like:
- "Connection refused" to 127.0.0.1
- "Polly.Timeout.TimeoutRejectedException"
- "The operation didn't complete within the allowed timeout"

This indicates that DCP cannot start properly. Common causes:
1. Docker is not running
2. Port conflicts (DCP needs available ports)
3. Insufficient permissions
4. Network configuration issues in CI environment

### Workarounds

If you need to run tests in an environment where DCP is not available:
1. Tests will automatically be marked as **Ignored** with a warning message
2. Consider using WebApplicationFactory for in-process API testing instead
3. Run integration tests only in environments with full Docker/DCP support

## Logging Configuration

Tests are configured with reduced logging to minimize console spam:
- Default: Information
- Microsoft.AspNetCore: Warning
- Aspire.Hosting.Dcp: Warning

## Test Structure

Each test follows this pattern:
1. **OneTimeSetUp**: Checks DCP availability before running any tests
2. **CreateAndStartAppHostAsync**: Helper method that creates and starts the AppHost
3. Individual test methods call the helper and perform assertions

##  Adding New Tests

When adding new integration tests:

```csharp
[Test]
public async Task YourTestName()
{
    // Arrange - This will skip the test if DCP is not available
    var (builder, app) = await CreateAndStartAppHostAsync();
    await using var _ = app;

    // Act
    var httpClient = app!.CreateHttpClient("your-service-name");
    var response = await httpClient.GetAsync("/your-endpoint");

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
}
```

## Further Reading

- [Aspire Testing Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/testing/overview)
- [Aspire Architecture Overview](https://learn.microsoft.com/en-us/dotnet/aspire/architecture/overview)
- [DCP (Developer Control Plane)](https://learn.microsoft.com/en-us/dotnet/aspire/architecture/overview#developer-control-plane)
