using Microsoft.EntityFrameworkCore;
using Zeitung.Api.Data;
using Zeitung.Api.DTOs;
using Zeitung.Api.Models;

namespace Zeitung.Api.Endpoints;

public static class ArticleEndpoints
{
    public static void MapArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/articles")
            .WithTags("Articles")
            .WithOpenApi();

        // GET /api/articles - List articles with filtering
        group.MapGet("/", async (
            ZeitungDbContext db, 
            int? userId, 
            int? feedId, 
            int? tagId,
            int page = 1,
            int pageSize = 20) =>
        {
            var query = db.Articles
                .Include(a => a.Feed)
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .AsQueryable();

            if (feedId.HasValue)
            {
                query = query.Where(a => a.FeedId == feedId.Value);
            }

            if (tagId.HasValue)
            {
                query = query.Where(a => a.ArticleTags.Any(at => at.TagId == tagId.Value));
            }

            var totalCount = await query.CountAsync();
            
            var articles = await query
                .OrderByDescending(a => a.PublishedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var articleDtos = articles.Select(a => new ArticleListDto(
                a.Id,
                a.Title,
                a.Link,
                a.Description,
                a.PublishedDate,
                a.Feed.Name,
                a.ArticleTags.Select(at => at.Tag.Name).ToList()
            )).ToList();

            return Results.Ok(new
            {
                Articles = articleDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        })
        .WithName("GetArticles")
        .WithSummary("Get articles")
        .WithDescription("Returns paginated articles with optional filtering by feed or tag");

        // GET /api/articles/{id} - Get article details
        group.MapGet("/{id}", async (int id, int? userId, ZeitungDbContext db) =>
        {
            var article = await db.Articles
                .Include(a => a.Feed)
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return Results.NotFound(new { message = "Article not found" });
            }

            int? userVote = null;
            if (userId.HasValue)
            {
                var vote = await db.Votes
                    .FirstOrDefaultAsync(v => v.UserId == userId.Value && v.ArticleId == id);
                userVote = vote?.Value;
            }

            var articleDto = new ArticleDto(
                article.Id,
                article.Title,
                article.Link,
                article.Description,
                article.PublishedDate,
                article.Feed.Name,
                article.ArticleTags.Select(at => new TagDto(
                    at.Tag.Id,
                    at.Tag.Name,
                    at.Confidence
                )).ToList(),
                userVote
            );

            return Results.Ok(articleDto);
        })
        .WithName("GetArticle")
        .WithSummary("Get article details")
        .WithDescription("Returns detailed information about a specific article");

        // POST /api/articles/{id}/vote - Vote on article
        group.MapPost("/{id}/vote", async (int id, VoteDto dto, int userId, ZeitungDbContext db) =>
        {
            if (dto.Value != 1 && dto.Value != -1)
            {
                return Results.BadRequest(new { message = "Vote value must be 1 or -1" });
            }

            var article = await db.Articles.FindAsync(id);
            if (article == null)
            {
                return Results.NotFound(new { message = "Article not found" });
            }

            var existingVote = await db.Votes
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ArticleId == id);

            if (existingVote != null)
            {
                if (existingVote.Value == dto.Value)
                {
                    // Remove vote if same value
                    db.Votes.Remove(existingVote);
                }
                else
                {
                    // Update vote if different value
                    existingVote.Value = dto.Value;
                }
            }
            else
            {
                // Create new vote
                db.Votes.Add(new Vote
                {
                    UserId = userId,
                    ArticleId = id,
                    Value = dto.Value
                });
            }

            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Vote recorded" });
        })
        .WithName("VoteArticle")
        .WithSummary("Vote on article")
        .WithDescription("Upvote or downvote an article");

        // POST /api/articles/{id}/like - Like article (updates tag interests)
        group.MapPost("/{id}/like", async (int id, int userId, ZeitungDbContext db) =>
        {
            var article = await db.Articles
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return Results.NotFound(new { message = "Article not found" });
            }

            // Update user tag preferences for all tags in this article
            foreach (var articleTag in article.ArticleTags)
            {
                var userTag = await db.UserTags
                    .FirstOrDefaultAsync(ut => 
                        ut.UserId == userId && 
                        ut.TagId == articleTag.TagId && 
                        ut.InteractionType == InteractionType.Liked);

                if (userTag != null)
                {
                    userTag.Score += articleTag.Confidence;
                    userTag.InteractionCount++;
                    userTag.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    db.UserTags.Add(new UserTag
                    {
                        UserId = userId,
                        TagId = articleTag.TagId,
                        InteractionType = InteractionType.Liked,
                        Score = articleTag.Confidence,
                        InteractionCount = 1
                    });
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Article liked, tag preferences updated" });
        })
        .WithName("LikeArticle")
        .WithSummary("Like article")
        .WithDescription("Marks article as liked and updates user's tag interest scores");

        // POST /api/articles/{id}/click - Track article click (updates tag interests)
        group.MapPost("/{id}/click", async (int id, int userId, ZeitungDbContext db) =>
        {
            var article = await db.Articles
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return Results.NotFound(new { message = "Article not found" });
            }

            // Update user tag preferences for all tags in this article
            foreach (var articleTag in article.ArticleTags)
            {
                var userTag = await db.UserTags
                    .FirstOrDefaultAsync(ut => 
                        ut.UserId == userId && 
                        ut.TagId == articleTag.TagId && 
                        ut.InteractionType == InteractionType.Clicked);

                if (userTag != null)
                {
                    userTag.Score += articleTag.Confidence * 0.5; // Lower weight for clicks
                    userTag.InteractionCount++;
                    userTag.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    db.UserTags.Add(new UserTag
                    {
                        UserId = userId,
                        TagId = articleTag.TagId,
                        InteractionType = InteractionType.Clicked,
                        Score = articleTag.Confidence * 0.5,
                        InteractionCount = 1
                    });
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Article click tracked" });
        })
        .WithName("ClickArticle")
        .WithSummary("Track article click")
        .WithDescription("Tracks article click and updates user's tag interest scores");
    }
}
