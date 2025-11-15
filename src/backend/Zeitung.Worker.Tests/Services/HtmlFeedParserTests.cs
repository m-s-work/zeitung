using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Services;

[TestFixture]
public class HtmlFeedParserTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Mock<ILogger<HtmlFeedParser>> _loggerMock = null!;
    private HtmlFeedParser _parser = null!;

    [SetUp]
    public void Setup()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<HtmlFeedParser>>();
        _parser = new HtmlFeedParser(_httpClientFactoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task ParseHtmlFeedAsync_WithBrutkastenSample_ParsesArticlesCorrectly()
    {
        // Arrange
        var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "brutkasten-sample.html");
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        
        var feed = new RssFeed
        {
            Name = "DerBrutkasten",
            Url = "https://brutkasten.com/news/newsticker",
            Type = "html5",
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article.newsticker-item",
                Title = new SelectorConfig
                {
                    Selector = "h3.title a",
                    Extractor = "text"
                },
                Link = new SelectorConfig
                {
                    Selector = "h3.title a",
                    Extractor = "href"
                },
                Description = new SelectorConfig
                {
                    Selector = "div.teaser",
                    Extractor = "text"
                },
                PublishedAt = new SelectorConfig
                {
                    Selector = "time",
                    Extractor = "datetime"
                },
                Category = new SelectorConfig
                {
                    Selector = "span.category",
                    Extractor = "text"
                }
            }
        };

        // Act
        var articles = await _parser.ParseHtmlFeedAsync(htmlContent, feed);

        // Assert
        Assert.That(articles, Is.Not.Null);
        Assert.That(articles, Has.Count.EqualTo(3));

        var firstArticle = articles[0];
        Assert.That(firstArticle.Title, Is.EqualTo("Österreichisches Startup erhält 5 Mio. Euro Funding"));
        Assert.That(firstArticle.Link, Does.Contain("startup-funding-2024"));
        Assert.That(firstArticle.Description, Does.Contain("Wiener Technologie-Startup"));
        Assert.That(firstArticle.PublishedDate.Year, Is.EqualTo(2024));
        Assert.That(firstArticle.PublishedDate.Month, Is.EqualTo(11));
        Assert.That(firstArticle.PublishedDate.Day, Is.EqualTo(13));
        Assert.That(firstArticle.Categories, Has.Count.EqualTo(2));
        Assert.That(firstArticle.Categories, Does.Contain("Startup"));
        Assert.That(firstArticle.Categories, Does.Contain("Funding"));
        Assert.That(firstArticle.FeedSource, Is.EqualTo("DerBrutkasten"));

        var secondArticle = articles[1];
        Assert.That(secondArticle.Title, Does.Contain("Grazer Unternehmen"));
        Assert.That(secondArticle.Categories, Does.Contain("Innovation"));
        Assert.That(secondArticle.Categories, Does.Contain("KI"));

        var thirdArticle = articles[2];
        Assert.That(thirdArticle.Title, Does.Contain("Gründerszene"));
        Assert.That(thirdArticle.Categories, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ParseHtmlFeedAsync_WithIngenieurSample_ParsesArticlesCorrectly()
    {
        // Arrange
        var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "ingenieur-sample.html");
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        
        var feed = new RssFeed
        {
            Name = "Ingenieur.de",
            Url = "https://www.ingenieur.de/technik/fachbereiche/bau/",
            Type = "html5",
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article.article-teaser",
                Title = new SelectorConfig
                {
                    Selector = "h2.headline a",
                    Extractor = "text"
                },
                Link = new SelectorConfig
                {
                    Selector = "h2.headline a",
                    Extractor = "href"
                },
                Description = new SelectorConfig
                {
                    Selector = "p.intro",
                    Extractor = "text"
                },
                PublishedAt = new SelectorConfig
                {
                    Selector = "time",
                    Extractor = "datetime"
                },
                Category = new SelectorConfig
                {
                    Selector = ".meta span.tag",
                    Extractor = "text"
                }
            }
        };

        // Act
        var articles = await _parser.ParseHtmlFeedAsync(htmlContent, feed);

        // Assert
        Assert.That(articles, Is.Not.Null);
        Assert.That(articles, Has.Count.EqualTo(2));

        var firstArticle = articles[0];
        Assert.That(firstArticle.Title, Does.Contain("Bautechnologie"));
        Assert.That(firstArticle.Link, Does.Contain("neue-bautechnologie-revolution"));
        Assert.That(firstArticle.Description, Does.Contain("Forscher"));
        Assert.That(firstArticle.PublishedDate.Year, Is.EqualTo(2024));
        Assert.That(firstArticle.Categories, Has.Count.EqualTo(2));
        Assert.That(firstArticle.Categories, Does.Contain("Bautechnik"));
        Assert.That(firstArticle.Categories, Does.Contain("Nachhaltigkeit"));
        Assert.That(firstArticle.FeedSource, Is.EqualTo("Ingenieur.de"));

        var secondArticle = articles[1];
        Assert.That(secondArticle.Title, Does.Contain("Digitalisierung"));
        Assert.That(secondArticle.Categories, Does.Contain("Digitalisierung"));
        Assert.That(secondArticle.Categories, Does.Contain("BIM"));
    }

    [Test]
    public async Task ParseFeedAsync_WithHttpRequest_FetchesAndParsesCorrectly()
    {
        // Arrange
        var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "brutkasten-sample.html");
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        
        var feed = new RssFeed
        {
            Name = "Test HTML Feed",
            Url = "https://example.com/news",
            Type = "html5",
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article.newsticker-item",
                Title = new SelectorConfig
                {
                    Selector = "h3.title a",
                    Extractor = "text"
                },
                Link = new SelectorConfig
                {
                    Selector = "h3.title a",
                    Extractor = "href"
                }
            }
        };

        var messageHandler = new Mock<HttpMessageHandler>();
        messageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(htmlContent)
            });

        var httpClient = new HttpClient(messageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act
        var articles = await _parser.ParseFeedAsync(feed);

        // Assert
        Assert.That(articles, Is.Not.Null);
        Assert.That(articles, Has.Count.EqualTo(3));
    }

    [Test]
    public void ParseFeedAsync_WithoutHtmlConfig_ThrowsException()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Invalid Feed",
            Url = "https://example.com/news",
            Type = "html5",
            HtmlConfig = null
        };

        var messageHandler = new Mock<HttpMessageHandler>();
        messageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("<html></html>")
            });

        var httpClient = new HttpClient(messageHandler.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _parser.ParseFeedAsync(feed));
    }

    [Test]
    public async Task ParseHtmlFeedAsync_WithEmptyHtml_ReturnsEmptyList()
    {
        // Arrange
        var htmlContent = "<html><body></body></html>";
        var feed = new RssFeed
        {
            Name = "Empty Feed",
            Url = "https://example.com/empty",
            Type = "html5",
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article.news",
                Title = new SelectorConfig { Selector = "h2", Extractor = "text" },
                Link = new SelectorConfig { Selector = "a", Extractor = "href" }
            }
        };

        // Act
        var articles = await _parser.ParseHtmlFeedAsync(htmlContent, feed);

        // Assert
        Assert.That(articles, Is.Not.Null);
        Assert.That(articles, Is.Empty);
    }

    [Test]
    public async Task ParseHtmlFeedAsync_WithRelativeUrls_ConvertsToAbsolute()
    {
        // Arrange
        var htmlContent = @"
<html>
<body>
    <article>
        <h2><a href=""/article/test"">Test Article</a></h2>
    </article>
</body>
</html>";
        
        var feed = new RssFeed
        {
            Name = "Relative URL Test",
            Url = "https://example.com/news/",
            Type = "html5",
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article",
                Title = new SelectorConfig { Selector = "h2 a", Extractor = "text" },
                Link = new SelectorConfig { Selector = "h2 a", Extractor = "href" }
            }
        };

        // Act
        var articles = await _parser.ParseHtmlFeedAsync(htmlContent, feed);

        // Assert
        Assert.That(articles, Has.Count.EqualTo(1));
        Assert.That(articles[0].Link, Does.StartWith("https://example.com/"));
        Assert.That(articles[0].Link, Does.Contain("/article/test"));
    }

    [Test]
    public async Task ParseHtmlFeedAsync_WithMissingTitleAndLink_SkipsItem()
    {
        // Arrange
        var htmlContent = @"
<html>
<body>
    <article>
        <p>Only description, no title or link</p>
    </article>
    <article>
        <h2><a href=""/valid"">Valid Article</a></h2>
    </article>
</body>
</html>";
        
        var feed = new RssFeed
        {
            Name = "Skip Test",
            Url = "https://example.com/",
            Type = "html5",
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article",
                Title = new SelectorConfig { Selector = "h2 a", Extractor = "text" },
                Link = new SelectorConfig { Selector = "h2 a", Extractor = "href" }
            }
        };

        // Act
        var articles = await _parser.ParseHtmlFeedAsync(htmlContent, feed);

        // Assert
        Assert.That(articles, Has.Count.EqualTo(1));
        Assert.That(articles[0].Title, Is.EqualTo("Valid Article"));
    }

    [Test]
    public async Task ParseHtmlFeedAsync_WithInvalidDateFormat_UsesDefaultDate()
    {
        // Arrange
        var htmlContent = @"
<html>
<body>
    <article>
        <h2><a href=""/article"">Test</a></h2>
        <time>Invalid Date</time>
    </article>
</body>
</html>";
        
        var feed = new RssFeed
        {
            Name = "Date Test",
            Url = "https://example.com/",
            Type = "html5",
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article",
                Title = new SelectorConfig { Selector = "h2 a", Extractor = "text" },
                Link = new SelectorConfig { Selector = "h2 a", Extractor = "href" },
                PublishedAt = new SelectorConfig { Selector = "time", Extractor = "text" }
            }
        };

        // Act
        var articles = await _parser.ParseHtmlFeedAsync(htmlContent, feed);

        // Assert
        Assert.That(articles, Has.Count.EqualTo(1));
        Assert.That(articles[0].PublishedDate, Is.EqualTo(default(DateTime)));
    }

    [Test]
    public void CanHandle_WithHtml5TypeAndConfig_ReturnsTrue()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "HTML Feed",
            Url = "https://example.com/",
            Type = "html5",
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article",
                Title = new SelectorConfig { Selector = "h2", Extractor = "text" },
                Link = new SelectorConfig { Selector = "a", Extractor = "href" }
            }
        };

        // Act
        var canHandle = _parser.CanHandle(feed);

        // Assert
        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_WithRssType_ReturnsFalse()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "RSS Feed",
            Url = "https://example.com/rss",
            Type = "rss"
        };

        // Act
        var canHandle = _parser.CanHandle(feed);

        // Assert
        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CanHandle_WithHtml5TypeButNoConfig_ReturnsFalse()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "HTML Feed",
            Url = "https://example.com/",
            Type = "html5",
            HtmlConfig = null
        };

        // Act
        var canHandle = _parser.CanHandle(feed);

        // Assert
        Assert.That(canHandle, Is.False);
    }
}
