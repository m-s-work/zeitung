using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Services;

[TestFixture]
public class FeedExpanderTests
{
    [Test]
    public void ExpandFeed_WithNoPatterns_ReturnsSingleFeed()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Test Feed",
            Url = "https://example.com/feed",
            Type = "rss"
        };

        // Act
        var expanded = FeedExpander.ExpandFeed(feed);

        // Assert
        Assert.That(expanded, Has.Count.EqualTo(1));
        Assert.That(expanded[0], Is.EqualTo(feed));
    }

    [Test]
    public void ExpandFeed_WithPatterns_ReturnsMultipleFeeds()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Test {pattern}",
            Url = "https://example.com/{pattern}/",
            Description = "Description for {pattern}",
            Type = "html5",
            UrlPatterns = new List<string> { "tech", "business", "sports" },
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article",
                Title = new SelectorConfig { Selector = "h2", Extractor = "text" },
                Link = new SelectorConfig { Selector = "a", Extractor = "href" }
            }
        };

        // Act
        var expanded = FeedExpander.ExpandFeed(feed);

        // Assert
        Assert.That(expanded, Has.Count.EqualTo(3));
        
        Assert.That(expanded[0].Name, Is.EqualTo("Test tech"));
        Assert.That(expanded[0].Url, Is.EqualTo("https://example.com/tech/"));
        Assert.That(expanded[0].Description, Is.EqualTo("Description for tech"));
        Assert.That(expanded[0].Type, Is.EqualTo("html5"));
        Assert.That(expanded[0].HtmlConfig, Is.Not.Null);

        Assert.That(expanded[1].Name, Is.EqualTo("Test business"));
        Assert.That(expanded[1].Url, Is.EqualTo("https://example.com/business/"));
        
        Assert.That(expanded[2].Name, Is.EqualTo("Test sports"));
        Assert.That(expanded[2].Url, Is.EqualTo("https://example.com/sports/"));
    }

    [Test]
    public void ExpandFeed_WithEmptyPatternsList_ReturnsSingleFeed()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Test Feed",
            Url = "https://example.com/feed",
            Type = "rss",
            UrlPatterns = new List<string>()
        };

        // Act
        var expanded = FeedExpander.ExpandFeed(feed);

        // Assert
        Assert.That(expanded, Has.Count.EqualTo(1));
        Assert.That(expanded[0], Is.EqualTo(feed));
    }

    [Test]
    public void ExpandFeeds_WithMixedFeeds_ExpandsOnlyThoseWithPatterns()
    {
        // Arrange
        var feeds = new List<RssFeed>
        {
            new RssFeed
            {
                Name = "Regular Feed",
                Url = "https://example.com/rss",
                Type = "rss"
            },
            new RssFeed
            {
                Name = "Pattern Feed {pattern}",
                Url = "https://example.com/{pattern}",
                Type = "html5",
                UrlPatterns = new List<string> { "a", "b" },
                HtmlConfig = new HtmlFeedConfig
                {
                    ItemsSelector = "article",
                    Title = new SelectorConfig { Selector = "h2", Extractor = "text" },
                    Link = new SelectorConfig { Selector = "a", Extractor = "href" }
                }
            }
        };

        // Act
        var expanded = FeedExpander.ExpandFeeds(feeds);

        // Assert
        Assert.That(expanded, Has.Count.EqualTo(3));
        Assert.That(expanded[0].Name, Is.EqualTo("Regular Feed"));
        Assert.That(expanded[1].Name, Is.EqualTo("Pattern Feed a"));
        Assert.That(expanded[2].Name, Is.EqualTo("Pattern Feed b"));
    }

    [Test]
    public void ExpandFeed_WithNullDescription_HandlesGracefully()
    {
        // Arrange
        var feed = new RssFeed
        {
            Name = "Test {pattern}",
            Url = "https://example.com/{pattern}/",
            Description = null,
            Type = "html5",
            UrlPatterns = new List<string> { "tech" },
            HtmlConfig = new HtmlFeedConfig
            {
                ItemsSelector = "article",
                Title = new SelectorConfig { Selector = "h2", Extractor = "text" },
                Link = new SelectorConfig { Selector = "a", Extractor = "href" }
            }
        };

        // Act
        var expanded = FeedExpander.ExpandFeed(feed);

        // Assert
        Assert.That(expanded, Has.Count.EqualTo(1));
        Assert.That(expanded[0].Description, Is.Null);
    }

    [Test]
    public void ExpandFeed_PreservesHtmlConfig()
    {
        // Arrange
        var htmlConfig = new HtmlFeedConfig
        {
            ItemsSelector = "article.news",
            Title = new SelectorConfig { Selector = "h2.title", Extractor = "text" },
            Link = new SelectorConfig { Selector = "a.link", Extractor = "href" },
            Description = new SelectorConfig { Selector = "p.desc", Extractor = "text" }
        };

        var feed = new RssFeed
        {
            Name = "Test {pattern}",
            Url = "https://example.com/{pattern}/",
            Type = "html5",
            UrlPatterns = new List<string> { "cat1", "cat2" },
            HtmlConfig = htmlConfig
        };

        // Act
        var expanded = FeedExpander.ExpandFeed(feed);

        // Assert
        Assert.That(expanded, Has.Count.EqualTo(2));
        Assert.That(expanded[0].HtmlConfig, Is.SameAs(htmlConfig));
        Assert.That(expanded[1].HtmlConfig, Is.SameAs(htmlConfig));
    }
}
