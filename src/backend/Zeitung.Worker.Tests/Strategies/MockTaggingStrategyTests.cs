using Zeitung.Core.Models;
using Zeitung.Worker.Models;
using ArticleDto = Zeitung.Worker.Models.Article;
using Zeitung.Worker.Strategies;

namespace Zeitung.Worker.Tests.Strategies;

[TestFixture]
public class MockTaggingStrategyTests
{
    private MockTaggingStrategy _strategy = null!;

    [SetUp]
    public void Setup()
    {
        _strategy = new MockTaggingStrategy();
    }

    [Test]
    public async Task GenerateTagsAsync_ReturnsExpectedMockTags()
    {
        // Arrange
        var article = new ArticleDto
        {
            Title = "Test Article",
            Link = "https://example.com/test",
            Description = "Test description",
            FeedSource = "Test Feed"
        };

        // Act
        var tags = await _strategy.GenerateTagsAsync(article);

        // Assert
        Assert.That(tags, Is.Not.Null);
        Assert.That(tags.Count, Is.EqualTo(3));
        Assert.That(tags, Does.Contain("mock-tag-1"));
        Assert.That(tags, Does.Contain("mock-tag-2"));
        Assert.That(tags, Does.Contain("test"));
    }

    [Test]
    public async Task GenerateTagsAsync_ReturnsSameTagsForAllArticles()
    {
        // Arrange
        var article1 = new ArticleDto
        {
            Title = "Article 1",
            Link = "https://example.com/1",
            Description = "Description 1",
            FeedSource = "Test Feed"
        };
        
        var article2 = new ArticleDto
        {
            Title = "Article 2",
            Link = "https://example.com/2",
            Description = "Description 2",
            FeedSource = "Test Feed"
        };

        // Act
        var tags1 = await _strategy.GenerateTagsAsync(article1);
        var tags2 = await _strategy.GenerateTagsAsync(article2);

        // Assert
        Assert.That(tags1, Is.EqualTo(tags2));
    }
}
