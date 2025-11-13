using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Zeitung.Core.Models;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Services;

[TestFixture]
public class RssFeedParserTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<ILogger<RssFeedParser>> _loggerMock = null!;
    private RssFeedParser _parser = null!;

    [SetUp]
    public void Setup()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<RssFeedParser>>();
        _parser = new RssFeedParser(_httpClientFactoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task ParseFeedAsync_WithValidXmlFile_ParsesArticlesCorrectly()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Test Feed",
            Url = "file://test-feed.xml",
            Description = "Test description"
        };

        var xmlContent = await File.ReadAllTextAsync("TestData/test-feed.xml");
        
        var messageHandler = new Mock<HttpMessageHandler>();
        messageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(xmlContent)
            });

        var httpClient = new HttpClient(messageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var articles = await _parser.ParseFeedAsync(feed);

        // Assert
        Assert.That(articles, Is.Not.Null);
        Assert.That(articles.Count, Is.EqualTo(3));
        
        var firstArticle = articles[0];
        Assert.That(firstArticle.Title, Is.EqualTo("Breaking: New Technology Breakthrough"));
        Assert.That(firstArticle.Link, Does.Contain("tech-breakthrough"));
        Assert.That(firstArticle.FeedSource, Is.EqualTo("Test Feed"));
        Assert.That(firstArticle.Categories, Does.Contain("Technology"));
        Assert.That(firstArticle.Categories, Does.Contain("Science"));
    }

    [Test]
    public async Task ParseFeedAsync_WithBBCSample_ParsesArticlesCorrectly()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "BBC News",
            Url = "file://bbc-news-sample.xml"
        };

        var xmlContent = await File.ReadAllTextAsync("TestData/bbc-news-sample.xml");
        
        var messageHandler = new Mock<HttpMessageHandler>();
        messageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(xmlContent)
            });

        var httpClient = new HttpClient(messageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var articles = await _parser.ParseFeedAsync(feed);

        // Assert
        Assert.That(articles, Is.Not.Null);
        Assert.That(articles.Count, Is.EqualTo(2));
        Assert.That(articles[0].Title, Does.Contain("diplomacy"));
    }

    [Test]
    public async Task ParseFeedAsync_WithHeiseSample_ParsesGermanContentCorrectly()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Heise Online",
            Url = "file://heise-sample.xml"
        };

        var xmlContent = await File.ReadAllTextAsync("TestData/heise-sample.xml");
        
        var messageHandler = new Mock<HttpMessageHandler>();
        messageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(xmlContent)
            });

        var httpClient = new HttpClient(messageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var articles = await _parser.ParseFeedAsync(feed);

        // Assert
        Assert.That(articles, Is.Not.Null);
        Assert.That(articles.Count, Is.EqualTo(2));
        Assert.That(articles[0].Categories, Does.Contain("Security"));
        Assert.That(articles[1].Categories, Does.Contain("AI"));
    }

    [Test]
    [Ignore("OBSOLETE: we want failure to actually fail now, not hide it with empty list")]
    public async Task ParseFeedAsync_WithHttpError_ReturnsEmptyList()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Error Feed",
            Url = "https://example.com/error"
        };

        var messageHandler = new Mock<HttpMessageHandler>();
        messageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(messageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var articles = await _parser.ParseFeedAsync(feed);

        // Assert
        Assert.That(articles, Is.Not.Null);
        Assert.That(articles, Is.Empty);
    }
}
