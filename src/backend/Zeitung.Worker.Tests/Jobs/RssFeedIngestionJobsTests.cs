using Microsoft.Extensions.Logging;
using Moq;
using Zeitung.Worker.Jobs;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Jobs;

[TestFixture]
public class RssFeedIngestionJobsTests
{
    private Mock<IFeedIngestService> _feedIngestServiceMock = null!;
    private Mock<ILogger<RssFeedIngestionJobs>> _loggerMock = null!;
    private RssFeedIngestionJobs _job = null!;

    [SetUp]
    public void Setup()
    {
        _feedIngestServiceMock = new Mock<IFeedIngestService>();
        _loggerMock = new Mock<ILogger<RssFeedIngestionJobs>>();
        _job = new RssFeedIngestionJobs(_feedIngestServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task IngestRssFeeds_CallsFeedIngestService()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        _feedIngestServiceMock
            .Setup(x => x.IngestFeedsAsync(cancellationToken))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _job.IngestRssFeeds(cancellationToken);

        // Assert
        _feedIngestServiceMock.Verify(
            x => x.IngestFeedsAsync(cancellationToken),
            Times.Once);
    }

    [Test]
    public async Task IngestRssFeeds_LogsStartAndCompletion()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        _feedIngestServiceMock
            .Setup(x => x.IngestFeedsAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _job.IngestRssFeeds(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("starting execution")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void IngestRssFeeds_WhenServiceThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Test exception");
        _feedIngestServiceMock
            .Setup(x => x.IngestFeedsAsync(cancellationToken))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _job.IngestRssFeeds(cancellationToken));
        
        Assert.That(ex, Is.EqualTo(expectedException));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void IngestRssFeeds_HandlesCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;

        _feedIngestServiceMock
            .Setup(x => x.IngestFeedsAsync(cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        var exception = Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _job.IngestRssFeeds(cancellationToken));
        
        Assert.That(exception, Is.Not.Null);
    }
}
