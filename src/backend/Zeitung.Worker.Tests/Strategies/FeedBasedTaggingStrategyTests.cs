using Zeitung.Core.Models;
using Zeitung.Worker.Strategies;

namespace Zeitung.Worker.Tests.Strategies;

[TestFixture]
public class FeedBasedTaggingStrategyTests
{
    private FeedBasedTaggingStrategy _strategy = null!;

    [SetUp]
    public void Setup()
    {
        _strategy = new FeedBasedTaggingStrategy();
    }

    [Test]
    public async Task GenerateTagsAsync_IncludesCategoriesFromArticle()
    {
        // Arrange
        var article = new Article
        {
            Title = "Breaking News Story",
            Link = "https://example.com/news",
            Description = "Important breaking news about technology",
            FeedSource = "Test Feed",
            Categories = new List<string> { "Technology", "Science" }
        };

        // Act
        var tags = await _strategy.GenerateTagsAsync(article);

        // Assert
        Assert.That(tags, Is.Not.Null);
        Assert.That(tags, Does.Contain("Technology"));
        Assert.That(tags, Does.Contain("Science"));
    }

    [Test]
    public async Task GenerateTagsAsync_ExtractsKeywordsFromTitle()
    {
        // Arrange
        var article = new Article
        {
            Title = "Artificial Intelligence Breakthrough",
            Link = "https://example.com/ai",
            Description = "Short description",
            FeedSource = "Test Feed"
        };

        // Act
        var tags = await _strategy.GenerateTagsAsync(article);

        // Assert
        Assert.That(tags, Is.Not.Null);
        Assert.That(tags.Any(t => t.Contains("artificial", StringComparison.OrdinalIgnoreCase)));
        Assert.That(tags.Any(t => t.Contains("intelligence", StringComparison.OrdinalIgnoreCase)));
        Assert.That(tags.Any(t => t.Contains("breakthrough", StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public async Task GenerateTagsAsync_FiltersShortWords()
    {
        // Arrange
        var article = new Article
        {
            Title = "AI is big in IT",
            Link = "https://example.com/test",
            Description = "A new era",
            FeedSource = "Test Feed"
        };

        // Act
        var tags = await _strategy.GenerateTagsAsync(article);

        // Assert
        // Short words (4 characters or less) should be filtered out
        Assert.That(tags, Is.All.Matches<string>(tag => tag.Length > 4));
    }

    [Test]
    public async Task GenerateTagsAsync_ReturnsUniqueTagsOnly()
    {
        // Arrange
        var article = new Article
        {
            Title = "Technology Technology revolution",
            Link = "https://example.com/test",
            Description = "Technology is changing everything",
            FeedSource = "Test Feed"
        };

        // Act
        var tags = await _strategy.GenerateTagsAsync(article);

        // Assert
        Assert.That(tags, Is.Unique);
    }
}
