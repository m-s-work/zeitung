using Microsoft.EntityFrameworkCore;
using Zeitung.Worker.Data;
using Zeitung.Worker.Models;

namespace Zeitung.Worker.Services;

public interface IArticleRepository
{
    Task<ArticleEntity?> GetByLinkAsync(string link, CancellationToken cancellationToken = default);
    Task<ArticleEntity> SaveAsync(Article article, CancellationToken cancellationToken = default);
}

public class ArticleRepository : IArticleRepository
{
    private readonly ZeitungDbContext _context;
    private readonly ILogger<ArticleRepository> _logger;

    public ArticleRepository(ZeitungDbContext context, ILogger<ArticleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ArticleEntity?> GetByLinkAsync(string link, CancellationToken cancellationToken = default)
    {
        return await _context.Articles
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(a => a.Link == link, cancellationToken);
    }

    public async Task<ArticleEntity> SaveAsync(Article article, CancellationToken cancellationToken = default)
    {
        // Check if article already exists
        var existingArticle = await GetByLinkAsync(article.Link, cancellationToken);
        if (existingArticle != null)
        {
            _logger.LogDebug("Article already exists: {Link}", article.Link);
            return existingArticle;
        }

        var articleEntity = new ArticleEntity
        {
            Title = article.Title,
            Link = article.Link,
            Description = article.Description,
            PublishedDate = article.PublishedDate,
            FeedSource = article.FeedSource,
            CreatedAt = DateTime.UtcNow
        };

        _context.Articles.Add(articleEntity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved article: {Title} (ID: {Id})", articleEntity.Title, articleEntity.Id);
        return articleEntity;
    }
}
