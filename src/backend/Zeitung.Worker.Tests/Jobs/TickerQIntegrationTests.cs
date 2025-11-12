using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TickerQ.DependencyInjection;
using Zeitung.Worker.Jobs;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Jobs;

[TestFixture]
public class TickerQIntegrationTests
{
    [Test]
    public void TickerQ_IsRegisteredCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Mock the feed ingest service
        var feedIngestServiceMock = new Mock<IFeedIngestService>();
        services.AddScoped<IFeedIngestService>(_ => feedIngestServiceMock.Object);
        
        // Register TickerQ (mimicking Program.cs)
        services.AddTickerQ(options =>
        {
            options.SetMaxConcurrency(5);
        });
        
        // Register job classes
        services.AddScoped<RssFeedIngestionJobs>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var job = serviceProvider.GetService<RssFeedIngestionJobs>();
        
        // Assert
        Assert.That(job, Is.Not.Null, "RssFeedIngestionJobs should be registered in DI container");
    }

    [Test]
    public async Task RssFeedIngestionJob_CanBeResolvedAndExecuted()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var feedIngestServiceMock = new Mock<IFeedIngestService>();
        feedIngestServiceMock
            .Setup(x => x.IngestFeedsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        services.AddScoped<IFeedIngestService>(_ => feedIngestServiceMock.Object);
        services.AddTickerQ(options =>
        {
            options.SetMaxConcurrency(5);
        });
        services.AddScoped<RssFeedIngestionJobs>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var job = serviceProvider.GetRequiredService<RssFeedIngestionJobs>();
        await job.IngestRssFeeds(CancellationToken.None);
        
        // Assert
        feedIngestServiceMock.Verify(
            x => x.IngestFeedsAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "FeedIngestService should be called once");
    }

    [Test]
    public void TickerQ_ServicesAreRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var feedIngestServiceMock = new Mock<IFeedIngestService>();
        services.AddScoped<IFeedIngestService>(_ => feedIngestServiceMock.Object);
        
        // Register TickerQ with specific max concurrency
        services.AddTickerQ(options =>
        {
            options.SetMaxConcurrency(5);
        });
        
        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify that key TickerQ services are registered
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType.Name.Contains("TickerQ"));
        Assert.That(serviceDescriptor, Is.Not.Null, "At least one TickerQ service should be registered");
    }

    [Test]
    public void JobClass_HasCorrectDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var feedIngestServiceMock = new Mock<IFeedIngestService>();
        services.AddScoped<IFeedIngestService>(_ => feedIngestServiceMock.Object);
        services.AddScoped<RssFeedIngestionJobs>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var job = serviceProvider.GetRequiredService<RssFeedIngestionJobs>();
        
        // Assert
        Assert.That(job, Is.Not.Null);
        Assert.That(job, Is.InstanceOf<RssFeedIngestionJobs>());
    }

    [Test]
    public async Task MultipleJobInstances_CanBeResolvedConcurrently()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var callCount = 0;
        var feedIngestServiceMock = new Mock<IFeedIngestService>();
        feedIngestServiceMock
            .Setup(x => x.IngestFeedsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                Interlocked.Increment(ref callCount);
                return Task.CompletedTask;
            });
        
        services.AddScoped<IFeedIngestService>(_ => feedIngestServiceMock.Object);
        services.AddTickerQ(options =>
        {
            options.SetMaxConcurrency(5);
        });
        services.AddScoped<RssFeedIngestionJobs>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act - Create multiple scopes and execute jobs concurrently
        var tasks = new List<Task>();
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var scope = serviceProvider.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<RssFeedIngestionJobs>();
                await job.IngestRssFeeds(CancellationToken.None);
            }));
        }

        await Task.WhenAll(tasks);
        
        // Assert
        Assert.That(callCount, Is.EqualTo(3), "All three job executions should complete");
    }
}
