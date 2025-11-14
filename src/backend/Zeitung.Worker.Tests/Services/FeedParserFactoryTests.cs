using Microsoft.Extensions.Logging;
using Moq;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Services;

[TestFixture]
public class FeedParserFactoryTests
{
    private Mock<ILogger<FeedParserFactory>> _loggerMock = null!;
    private Mock<IRssFeedParser> _rssFeedParserMock = null!;
    private Mock<IHtmlFeedParser> _htmlFeedParserMock = null!;
    private FeedParserFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<FeedParserFactory>>();
        _rssFeedParserMock = new Mock<IRssFeedParser>();
        _htmlFeedParserMock = new Mock<IHtmlFeedParser>();
        
        // Setup mock behavior
        _rssFeedParserMock.Setup(p => p.CanHandle(It.IsAny<RssFeed>()))
            .Returns<RssFeed>(feed => 
                string.IsNullOrEmpty(feed.Type) || 
                feed.Type.Equals("rss", StringComparison.OrdinalIgnoreCase));
        
        _htmlFeedParserMock.Setup(p => p.CanHandle(It.IsAny<RssFeed>()))
            .Returns<RssFeed>(feed => 
                feed.Type.Equals("html5", StringComparison.OrdinalIgnoreCase) && 
                feed.HtmlConfig != null);
        
        var parsers = new List<IFeedParser> 
        { 
            _rssFeedParserMock.Object, 
            _htmlFeedParserMock.Object 
        };
        
        _factory = new FeedParserFactory(parsers, _loggerMock.Object);
    }

    [Test]
    public void GetParser_WithRssFeed_ReturnsRssParser()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "RSS Feed",
            Url = "https://example.com/rss",
            Type = "rss"
        };

        // Act
        var parser = _factory.GetParser(feed);

        // Assert
        Assert.That(parser, Is.Not.Null);
        Assert.That(parser, Is.EqualTo(_rssFeedParserMock.Object));
    }

    [Test]
    public void GetParser_WithDefaultType_ReturnsRssParser()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Default Feed",
            Url = "https://example.com/feed"
            // Type defaults to "rss"
        };

        // Act
        var parser = _factory.GetParser(feed);

        // Assert
        Assert.That(parser, Is.Not.Null);
        Assert.That(parser, Is.EqualTo(_rssFeedParserMock.Object));
    }

    [Test]
    public void GetParser_WithHtml5FeedAndConfig_ReturnsHtmlParser()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "HTML5 Feed",
            Url = "https://example.com/news",
            Type = "html5",
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article",
                Title = new SelectorConfig { Selector = "h2", Extractor = "text" },
                Link = new SelectorConfig { Selector = "a", Extractor = "href" }
            }
        };

        // Act
        var parser = _factory.GetParser(feed);

        // Assert
        Assert.That(parser, Is.Not.Null);
        Assert.That(parser, Is.EqualTo(_htmlFeedParserMock.Object));
    }

    [Test]
    public void GetParser_WithHtml5FeedButNoConfig_ThrowsException()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Invalid HTML5 Feed",
            Url = "https://example.com/news",
            Type = "html5",
            HtmlConfig = null
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _factory.GetParser(feed));
        Assert.That(ex.Message, Does.Contain("No parser found"));
    }

    [Test]
    public void GetParser_WithUnsupportedType_ThrowsException()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Unsupported Feed",
            Url = "https://example.com/feed",
            Type = "json"
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _factory.GetParser(feed));
        Assert.That(ex.Message, Does.Contain("No parser found"));
        Assert.That(ex.Message, Does.Contain("json"));
    }

    [Test]
    public void GetParser_WithNullFeed_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _factory.GetParser(null!));
    }
}
