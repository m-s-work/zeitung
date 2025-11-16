using Microsoft.EntityFrameworkCore;
using Zeitung.Core.Context;
using Zeitung.Core.Models;
using ArticleDto = Zeitung.Worker.Models.Article;

namespace Zeitung.Worker.Services;

public interface IArticleRepository
{
    Task<Article?> GetByLinkAsync(string link, CancellationToken cancellationToken = default);
    Task<Article> SaveAsync(ArticleDto article, CancellationToken cancellationToken = default);
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

    public async Task<Article?> GetByLinkAsync(string link, CancellationToken cancellationToken = default)
    {
        return await _context.Articles
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(a => a.Link == link, cancellationToken);
    }

    public async Task<Article> SaveAsync(ArticleDto article, CancellationToken cancellationToken = default)
    {
        // Check if article already exists
        var existingArticle = await GetByLinkAsync(article.Link, cancellationToken);
        if (existingArticle != null)
        {
            _logger.LogDebug("Article already exists: {Link}", article.Link);
            return existingArticle;
        }

        // Find or create feed
        var feed = await _context.Feeds
            .FirstOrDefaultAsync(f => f.Url == article.FeedSource || f.Name == article.FeedSource, cancellationToken);
        
        if (feed == null)
        {
            feed = new Feed
            {
                Url = article.FeedSource,
                Name = article.FeedSource,
                Description = $"Auto-created feed from {article.FeedSource}",
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Feeds.Add(feed);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var articleEntity = new Article
        {
            Title = article.Title,
            Link = article.Link,
            Description = article.Description,
            PublishedDate = article.PublishedDate,
            FeedId = feed.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Articles.Add(articleEntity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved article: {Title} (ID: {Id})", articleEntity.Title, articleEntity.Id);
        return articleEntity;
    }
}
