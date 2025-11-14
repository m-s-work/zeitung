using Microsoft.Extensions.Logging;
using Moq;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Services;

[TestFixture]
public class RdfFeedParserTests
{
    private RdfFeedParser _parser = null!;
    private Mock<ILogger<RdfFeedParser>> _loggerMock = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<RdfFeedParser>>();
        _parser = new RdfFeedParser(_loggerMock.Object);
    }

    [Test]
    public async Task ParseRdfFeedAsync_WithValidRdfSample_ParsesArticlesCorrectly()
    {
        // Arrange
        var xmlPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "orf-rdf-sample.xml");
        var xmlContent = await File.ReadAllTextAsync(xmlPath);
        
        var feed = new RssFeed
        {
            Name = "ORF Test",
            Url = "https://rss.orf.at/news.xml"
        };

        // Act
        var articles = await _parser.ParseRdfFeedAsync(xmlContent, feed);

        // Assert
        Assert.That(articles, Is.Not.Null);
        Assert.That(articles, Has.Count.EqualTo(2));

        var firstArticle = articles[0];
        Assert.That(firstArticle.Title, Is.EqualTo("Österreich startet neue Klimainitiative"));
        Assert.That(firstArticle.Link, Is.EqualTo("https://orf.at/stories/3001234/"));
        Assert.That(firstArticle.Description, Does.Contain("Regierung"));
        Assert.That(firstArticle.Categories, Has.Count.EqualTo(2));
        Assert.That(firstArticle.Categories, Does.Contain("Politik"));
        Assert.That(firstArticle.Categories, Does.Contain("Umwelt"));
        Assert.That(firstArticle.FeedSource, Is.EqualTo("ORF Test"));

        var secondArticle = articles[1];
        Assert.That(secondArticle.Title, Is.EqualTo("Wiener Philharmoniker mit Jubiläumskonzert"));
        Assert.That(secondArticle.Link, Is.EqualTo("https://orf.at/stories/3001235/"));
        Assert.That(secondArticle.Description, Does.Contain("Musikverein"));
        Assert.That(secondArticle.Categories, Has.Count.EqualTo(2));
        Assert.That(secondArticle.Categories, Does.Contain("Kultur"));
        Assert.That(secondArticle.Categories, Does.Contain("Musik"));
    }

    [Test]
    public async Task ParseRdfFeedAsync_WithEmptyContent_ReturnsEmptyList()
    {
        // Arrange
        var xmlContent = "<?xml version=\"1.0\"?><rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"></rdf:RDF>";
        var feed = new RssFeed
        {
            Name = "Empty Feed",
            Url = "https://example.com/empty"
        };

        // Act
        var articles = await _parser.ParseRdfFeedAsync(xmlContent, feed);

        // Assert
        Assert.That(articles, Is.Not.Null);
        Assert.That(articles, Is.Empty);
    }

    [Test]
    public async Task ParseRdfFeedAsync_WithItemMissingTitleAndLink_SkipsItem()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0""?>
<rdf:RDF xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#"" xmlns=""http://purl.org/rss/1.0/"">
  <item>
    <description>Only description, no title or link</description>
  </item>
  <item>
    <title>Valid Article</title>
    <link>https://example.com/article</link>
  </item>
</rdf:RDF>";
        
        var feed = new RssFeed
        {
            Name = "Test Feed",
            Url = "https://example.com/feed"
        };

        // Act
        var articles = await _parser.ParseRdfFeedAsync(xmlContent, feed);

        // Assert
        Assert.That(articles, Has.Count.EqualTo(1));
        Assert.That(articles[0].Title, Is.EqualTo("Valid Article"));
    }

    [Test]
    public async Task ParseRdfFeedAsync_WithDcDateFormat_ParsesDateCorrectly()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0""?>
<rdf:RDF xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#"" 
         xmlns=""http://purl.org/rss/1.0/""
         xmlns:dc=""http://purl.org/dc/elements/1.1/"">
  <item>
    <title>Article with DC Date</title>
    <link>https://example.com/article</link>
    <dc:date>2024-11-13T10:30:00+01:00</dc:date>
  </item>
</rdf:RDF>";
        
        var feed = new RssFeed
        {
            Name = "Date Test Feed",
            Url = "https://example.com/feed"
        };

        // Act
        var articles = await _parser.ParseRdfFeedAsync(xmlContent, feed);

        // Assert
        Assert.That(articles, Has.Count.EqualTo(1));
        Assert.That(articles[0].PublishedDate.Year, Is.EqualTo(2024));
        Assert.That(articles[0].PublishedDate.Month, Is.EqualTo(11));
        Assert.That(articles[0].PublishedDate.Day, Is.EqualTo(13));
    }

    [Test]
    public async Task ParseRdfFeedAsync_WithMultipleCategories_ParsesAllCategories()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0""?>
<rdf:RDF xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#"" 
         xmlns=""http://purl.org/rss/1.0/""
         xmlns:dc=""http://purl.org/dc/elements/1.1/"">
  <item>
    <title>Multi-Category Article</title>
    <link>https://example.com/article</link>
    <dc:subject>Category1</dc:subject>
    <dc:subject>Category2</dc:subject>
    <dc:subject>Category3</dc:subject>
  </item>
</rdf:RDF>";
        
        var feed = new RssFeed
        {
            Name = "Category Test Feed",
            Url = "https://example.com/feed"
        };

        // Act
        var articles = await _parser.ParseRdfFeedAsync(xmlContent, feed);

        // Assert
        Assert.That(articles, Has.Count.EqualTo(1));
        Assert.That(articles[0].Categories, Has.Count.EqualTo(3));
        Assert.That(articles[0].Categories, Does.Contain("Category1"));
        Assert.That(articles[0].Categories, Does.Contain("Category2"));
        Assert.That(articles[0].Categories, Does.Contain("Category3"));
    }
}
