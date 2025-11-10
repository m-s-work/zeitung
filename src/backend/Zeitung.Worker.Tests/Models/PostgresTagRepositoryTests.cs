using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Zeitung.Core.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Models;

[TestFixture]
public class PostgresTagRepositoryTests
{
    private ZeitungDbContext _context = null!;
    private PostgresTagRepository _repository = null!;
    private Mock<ILogger<PostgresTagRepository>> _loggerMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ZeitungDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ZeitungDbContext(options);
        _loggerMock = new Mock<ILogger<PostgresTagRepository>>();
        _repository = new PostgresTagRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task SaveArticleTagsAsync_NewTags_CreatesTagsAndRelationships()
    {
        // Arrange
        var articleEntity = new ArticleEntity
        {
            Title = "Test Article",
            Link = "https://example.com/test",
            Description = "Test description",
            PublishedDate = DateTime.UtcNow,
            FeedSource = "Test Feed"
        };
        _context.Articles.Add(articleEntity);
        await _context.SaveChangesAsync();

        var tags = new List<string> { "technology", "news", "programming" };

        // Act
        await _repository.SaveArticleTagsAsync(articleEntity.Id, tags);

        // Assert
        var savedTags = await _context.Tags.ToListAsync();
        Assert.That(savedTags.Count, Is.EqualTo(3));
        
        var articleTags = await _context.ArticleTags
            .Where(at => at.ArticleId == articleEntity.Id)
            .ToListAsync();
        Assert.That(articleTags.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task SaveArticleTagsAsync_ExistingTags_ReusesTagsAndCreatesRelationships()
    {
        // Arrange
        var existingTag = new TagEntity { Name = "technology" };
        _context.Tags.Add(existingTag);
        await _context.SaveChangesAsync();

        var articleEntity = new ArticleEntity
        {
            Title = "Test Article",
            Link = "https://example.com/test",
            Description = "Test description",
            PublishedDate = DateTime.UtcNow,
            FeedSource = "Test Feed"
        };
        _context.Articles.Add(articleEntity);
        await _context.SaveChangesAsync();

        var tags = new List<string> { "technology", "news" };

        // Act
        await _repository.SaveArticleTagsAsync(articleEntity.Id, tags);

        // Assert
        var savedTags = await _context.Tags.ToListAsync();
        Assert.That(savedTags.Count, Is.EqualTo(2)); // Should reuse existing "technology" tag
    }

    [Test]
    public async Task SaveArticleTagsAsync_MultipleTags_CreatesCoOccurrenceRecords()
    {
        // Arrange
        var articleEntity = new ArticleEntity
        {
            Title = "Test Article",
            Link = "https://example.com/test",
            Description = "Test description",
            PublishedDate = DateTime.UtcNow,
            FeedSource = "Test Feed"
        };
        _context.Articles.Add(articleEntity);
        await _context.SaveChangesAsync();

        var tags = new List<string> { "technology", "news", "programming" };

        // Act
        await _repository.SaveArticleTagsAsync(articleEntity.Id, tags);

        // Assert
        var coOccurrences = await _context.TagCoOccurrences.ToListAsync();
        Assert.That(coOccurrences.Count, Is.EqualTo(3)); // 3 pairs: (technology,news), (technology,programming), (news,programming)
        
        foreach (var co in coOccurrences)
        {
            Assert.That(co.OccurrenceCount, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task SaveArticleTagsAsync_SameTagPairTwice_IncrementsCoOccurrenceCount()
    {
        // Arrange
        var article1 = new ArticleEntity
        {
            Title = "Test Article 1",
            Link = "https://example.com/test1",
            Description = "Test description",
            PublishedDate = DateTime.UtcNow,
            FeedSource = "Test Feed"
        };
        _context.Articles.Add(article1);

        var article2 = new ArticleEntity
        {
            Title = "Test Article 2",
            Link = "https://example.com/test2",
            Description = "Test description",
            PublishedDate = DateTime.UtcNow,
            FeedSource = "Test Feed"
        };
        _context.Articles.Add(article2);
        await _context.SaveChangesAsync();

        var tags = new List<string> { "technology", "news" };

        // Act
        await _repository.SaveArticleTagsAsync(article1.Id, tags);
        await _repository.SaveArticleTagsAsync(article2.Id, tags);

        // Assert
        var coOccurrences = await _context.TagCoOccurrences.ToListAsync();
        Assert.That(coOccurrences.Count, Is.EqualTo(1)); // Only one pair
        Assert.That(coOccurrences[0].OccurrenceCount, Is.EqualTo(2)); // Count incremented
    }

    [Test]
    public async Task GetAllTagsAsync_ReturnsAllTagNames()
    {
        // Arrange
        _context.Tags.AddRange(
            new TagEntity { Name = "technology" },
            new TagEntity { Name = "news" },
            new TagEntity { Name = "programming" }
        );
        await _context.SaveChangesAsync();

        // Act
        var tags = await _repository.GetAllTagsAsync();

        // Assert
        Assert.That(tags.Count, Is.EqualTo(3));
        Assert.That(tags, Does.Contain("technology"));
        Assert.That(tags, Does.Contain("news"));
        Assert.That(tags, Does.Contain("programming"));
    }

    [Test]
    public async Task SaveArticleTagsAsync_EmptyTagList_DoesNothing()
    {
        // Arrange
        var articleEntity = new ArticleEntity
        {
            Title = "Test Article",
            Link = "https://example.com/test",
            Description = "Test description",
            PublishedDate = DateTime.UtcNow,
            FeedSource = "Test Feed"
        };
        _context.Articles.Add(articleEntity);
        await _context.SaveChangesAsync();

        var emptyTags = new List<string>();

        // Act
        await _repository.SaveArticleTagsAsync(articleEntity.Id, emptyTags);

        // Assert
        var savedTags = await _context.Tags.ToListAsync();
        Assert.That(savedTags.Count, Is.EqualTo(0));
    }
}
