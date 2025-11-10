using Microsoft.EntityFrameworkCore;
using Zeitung.Core.Models;
using Zeitung.Api.DTOs;
using Zeitung.Core.Models;

namespace Zeitung.Api.Endpoints;

public static class FeedEndpoints
{
    public static void MapFeedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feeds")
            .WithTags("Feeds")
            .WithOpenApi();

        // GET /api/feeds - List all feeds (global + user's personal)
        group.MapGet("/", async (ZeitungDbContext db, int? userId) =>
        {
            var feeds = await db.Feeds
                .Include(f => f.UserFeeds)
                .ToListAsync();

            var feedDtos = feeds.Select(f => new FeedDto(
                f.Id,
                f.Url,
                f.Name,
                f.Description,
                f.IsApproved,
                userId.HasValue && f.UserFeeds.Any(uf => uf.UserId == userId.Value),
                f.CreatedAt,
                f.LastFetchedAt
            )).ToList();

            return Results.Ok(feedDtos);
        })
        .WithName("GetFeeds")
        .WithSummary("Get all feeds")
        .WithDescription("Returns all feeds including global (approved) and personal feeds");

        // POST /api/feeds - Add feed to personal list
        group.MapPost("/", async (CreateFeedDto dto, int userId, ZeitungDbContext db) =>
        {
            // Check if feed already exists
            var existingFeed = await db.Feeds.FirstOrDefaultAsync(f => f.Url == dto.Url);
            
            Feed feed;
            if (existingFeed != null)
            {
                feed = existingFeed;
            }
            else
            {
                feed = new Feed
                {
                    Url = dto.Url,
                    Name = dto.Name,
                    Description = dto.Description,
                    AddedByUserId = userId,
                    IsApproved = false
                };
                db.Feeds.Add(feed);
                await db.SaveChangesAsync();
            }

            // Check if user already subscribed
            var existingSubscription = await db.UserFeeds
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.FeedId == feed.Id);

            if (existingSubscription != null)
            {
                return Results.Conflict(new { message = "Already subscribed to this feed" });
            }

            var userFeed = new UserFeed
            {
                UserId = userId,
                FeedId = feed.Id
            };
            db.UserFeeds.Add(userFeed);
            await db.SaveChangesAsync();

            var feedDto = new FeedDto(
                feed.Id,
                feed.Url,
                feed.Name,
                feed.Description,
                feed.IsApproved,
                true,
                feed.CreatedAt,
                feed.LastFetchedAt
            );

            return Results.Created($"/api/feeds/{feed.Id}", feedDto);
        })
        .WithName("CreateFeed")
        .WithSummary("Add feed to personal list")
        .WithDescription("Adds a feed to the user's personal feed list");

        // DELETE /api/feeds/{id} - Remove feed from personal list
        group.MapDelete("/{id}", async (int id, int userId, ZeitungDbContext db) =>
        {
            var userFeed = await db.UserFeeds
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.FeedId == id);

            if (userFeed == null)
            {
                return Results.NotFound(new { message = "Feed subscription not found" });
            }

            db.UserFeeds.Remove(userFeed);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteFeed")
        .WithSummary("Remove feed from personal list")
        .WithDescription("Removes a feed from the user's personal feed list");

        // POST /api/feeds/{id}/promote - Promote feed to global (admin/mod)
        group.MapPost("/{id}/promote", async (int id, ZeitungDbContext db) =>
        {
            var feed = await db.Feeds.FindAsync(id);
            
            if (feed == null)
            {
                return Results.NotFound(new { message = "Feed not found" });
            }

            if (feed.IsApproved)
            {
                return Results.Conflict(new { message = "Feed is already approved" });
            }

            feed.IsApproved = true;
            feed.ApprovedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Feed promoted to global list" });
        })
        .WithName("PromoteFeed")
        .WithSummary("Promote feed to global list")
        .WithDescription("Promotes a feed to the global list (requires admin/mod role)");

        // GET /api/feeds/recommendations - Get feed recommendations
        group.MapGet("/recommendations", async (int userId, ZeitungDbContext db) =>
        {
            // Get user's tag preferences
            var userTags = await db.UserTags
                .Where(ut => ut.UserId == userId && ut.InteractionType != InteractionType.Ignored)
                .Include(ut => ut.Tag)
                .OrderByDescending(ut => ut.Score)
                .Take(20)
                .ToListAsync();

            if (!userTags.Any())
            {
                // Return popular feeds if user has no preferences
                var popularFeeds = await db.Feeds
                    .Where(f => f.IsApproved)
                    .Take(10)
                    .ToListAsync();

                return Results.Ok(popularFeeds.Select(f => new FeedRecommendationDto(
                    f.Id,
                    f.Url,
                    f.Name,
                    f.Description,
                    0.5,
                    new List<string>()
                )).ToList());
            }

            var userTagIds = userTags.Select(ut => ut.TagId).ToList();

            // Find feeds with articles that match user's interests
            var recommendedFeeds = await db.Feeds
                .Where(f => !db.UserFeeds.Any(uf => uf.UserId == userId && uf.FeedId == f.Id))
                .Where(f => f.Articles.Any(a => a.ArticleTags.Any(at => userTagIds.Contains(at.TagId))))
                .Select(f => new
                {
                    Feed = f,
                    RelevantTags = f.Articles
                        .SelectMany(a => a.ArticleTags)
                        .Where(at => userTagIds.Contains(at.TagId))
                        .Select(at => at.Tag.Name)
                        .Distinct()
                        .ToList(),
                    Score = f.Articles
                        .SelectMany(a => a.ArticleTags)
                        .Where(at => userTagIds.Contains(at.TagId))
                        .Sum(at => at.Confidence)
                })
                .OrderByDescending(x => x.Score)
                .Take(10)
                .ToListAsync();

            var recommendations = recommendedFeeds.Select(x => new FeedRecommendationDto(
                x.Feed.Id,
                x.Feed.Url,
                x.Feed.Name,
                x.Feed.Description,
                Math.Min(x.Score / 10.0, 1.0), // Normalize score
                x.RelevantTags
            )).ToList();

            return Results.Ok(recommendations);
        })
        .WithName("GetFeedRecommendations")
        .WithSummary("Get feed recommendations")
        .WithDescription("Returns feed recommendations based on user's tag preferences");
    }
}
