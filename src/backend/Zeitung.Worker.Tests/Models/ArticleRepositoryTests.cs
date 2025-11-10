using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Models;

[TestFixture]
public class ArticleRepositoryTests
{
    private ZeitungDbContext _context = null!;
    private ArticleRepository _repository = null!;
    private Mock<ILogger<ArticleRepository>> _loggerMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ZeitungDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ZeitungDbContext(options);
        _loggerMock = new Mock<ILogger<ArticleRepository>>();
        _repository = new ArticleRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task SaveAsync_NewArticle_SavesSuccessfully()
    {
        // Arrange
        var article = new Article
        {
            Title = "Test Article",
            Link = "https://example.com/test",
            Description = "Test description",
            PublishedDate = DateTime.UtcNow,
            FeedSource = "Test Feed"
        };

        // Act
        var result = await _repository.SaveAsync(article);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.Title, Is.EqualTo(article.Title));
        Assert.That(result.Link, Is.EqualTo(article.Link));
    }

    [Test]
    public async Task SaveAsync_DuplicateLink_ReturnsExistingArticle()
    {
        // Arrange
        var article1 = new Article
        {
            Title = "Test Article 1",
            Link = "https://example.com/test",
            Description = "Test description 1",
            PublishedDate = DateTime.UtcNow,
            FeedSource = "Test Feed"
        };

        var article2 = new Article
        {
            Title = "Test Article 2 (Different Title)",
            Link = "https://example.com/test", // Same link
            Description = "Test description 2",
            PublishedDate = DateTime.UtcNow,
            FeedSource = "Test Feed"
        };

        // Act
        var result1 = await _repository.SaveAsync(article1);
        var result2 = await _repository.SaveAsync(article2);

        // Assert
        Assert.That(result1.Id, Is.EqualTo(result2.Id));
        Assert.That(result1.Title, Is.EqualTo(article1.Title)); // Should keep original title
    }

    [Test]
    public async Task GetByLinkAsync_ExistingArticle_ReturnsArticle()
    {
        // Arrange
        var article = new Article
        {
            Title = "Test Article",
            Link = "https://example.com/test",
            Description = "Test description",
            PublishedDate = DateTime.UtcNow,
            FeedSource = "Test Feed"
        };

        await _repository.SaveAsync(article);

        // Act
        var result = await _repository.GetByLinkAsync("https://example.com/test");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Link, Is.EqualTo(article.Link));
        Assert.That(result.Title, Is.EqualTo(article.Title));
    }

    [Test]
    public async Task GetByLinkAsync_NonExistingArticle_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByLinkAsync("https://example.com/nonexistent");

        // Assert
        Assert.That(result, Is.Null);
    }
}
