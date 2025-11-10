using Microsoft.EntityFrameworkCore;
using Zeitung.Api.Data;
using Zeitung.Api.DTOs;
using Zeitung.Api.Models;

namespace Zeitung.Api.Endpoints;

public static class TagEndpoints
{
    public static void MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags")
            .WithTags("Tags")
            .WithOpenApi();

        // GET /api/tags - List all tags
        group.MapGet("/", async (ZeitungDbContext db, int page = 1, int pageSize = 50) =>
        {
            var totalCount = await db.Tags.CountAsync();
            
            var tags = await db.Tags
                .OrderByDescending(t => t.UsageCount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.UsageCount
                })
                .ToListAsync();

            return Results.Ok(new
            {
                Tags = tags,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        })
        .WithName("GetTags")
        .WithSummary("Get all tags")
        .WithDescription("Returns paginated list of all tags ordered by usage count");

        // GET /api/tags/user - Get user's tag preferences
        group.MapGet("/user", async (int userId, ZeitungDbContext db) =>
        {
            var userTags = await db.UserTags
                .Where(ut => ut.UserId == userId)
                .Include(ut => ut.Tag)
                .OrderByDescending(ut => ut.Score)
                .ToListAsync();

            var preferences = userTags.Select(ut => new UserTagPreferenceDto(
                ut.TagId,
                ut.Tag.Name,
                ut.InteractionType,
                ut.Score,
                ut.InteractionCount
            )).ToList();

            return Results.Ok(preferences);
        })
        .WithName("GetUserTags")
        .WithSummary("Get user's tag preferences")
        .WithDescription("Returns user's tag preferences grouped by interaction type");

        // POST /api/tags/interest - Mark tags as interesting
        group.MapPost("/interest", async (TagInterestDto dto, int userId, ZeitungDbContext db) =>
        {
            foreach (var tagId in dto.TagIds)
            {
                var tag = await db.Tags.FindAsync(tagId);
                if (tag == null)
                {
                    continue;
                }

                var userTag = await db.UserTags
                    .FirstOrDefaultAsync(ut => 
                        ut.UserId == userId && 
                        ut.TagId == tagId && 
                        ut.InteractionType == InteractionType.Explicit);

                if (userTag != null)
                {
                    userTag.Score += 1.0;
                    userTag.InteractionCount++;
                    userTag.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    db.UserTags.Add(new UserTag
                    {
                        UserId = userId,
                        TagId = tagId,
                        InteractionType = InteractionType.Explicit,
                        Score = 1.0,
                        InteractionCount = 1
                    });
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Tag interests updated" });
        })
        .WithName("MarkTagsInterest")
        .WithSummary("Mark tags as interesting")
        .WithDescription("Marks tags as explicitly interesting to the user");

        // POST /api/tags/ignore - Mark tags to ignore
        group.MapPost("/ignore", async (TagInterestDto dto, int userId, ZeitungDbContext db) =>
        {
            foreach (var tagId in dto.TagIds)
            {
                var tag = await db.Tags.FindAsync(tagId);
                if (tag == null)
                {
                    continue;
                }

                var userTag = await db.UserTags
                    .FirstOrDefaultAsync(ut => 
                        ut.UserId == userId && 
                        ut.TagId == tagId && 
                        ut.InteractionType == InteractionType.Ignored);

                if (userTag != null)
                {
                    userTag.Score = -10.0; // Strong negative signal
                    userTag.InteractionCount++;
                    userTag.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    db.UserTags.Add(new UserTag
                    {
                        UserId = userId,
                        TagId = tagId,
                        InteractionType = InteractionType.Ignored,
                        Score = -10.0,
                        InteractionCount = 1
                    });
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Tags marked as ignored" });
        })
        .WithName("IgnoreTags")
        .WithSummary("Mark tags to ignore")
        .WithDescription("Marks tags as explicitly ignored by the user");

        // GET /api/tags/summary - Get tag summary (with decay)
        group.MapGet("/summary", async (int userId, ZeitungDbContext db) =>
        {
            var userTags = await db.UserTags
                .Where(ut => ut.UserId == userId)
                .Include(ut => ut.Tag)
                .ToListAsync();

            // Apply time-based decay
            var now = DateTime.UtcNow;
            var decayHalfLife = TimeSpan.FromDays(30); // Interests decay with 30-day half-life
            
            var tagScores = new Dictionary<string, double>();
            
            foreach (var userTag in userTags)
            {
                // Calculate decay factor
                var daysSinceUpdate = (now - userTag.UpdatedAt).TotalDays;
                var halfLives = daysSinceUpdate / decayHalfLife.TotalDays;
                var decayFactor = Math.Pow(0.5, halfLives);
                
                var decayedScore = userTag.Score * decayFactor;
                
                // Skip ignored tags and very low scores
                if (userTag.InteractionType == InteractionType.Ignored || decayedScore < 0.1)
                {
                    continue;
                }
                
                tagScores[userTag.Tag.Name] = decayedScore;
            }

            // Sort by score
            var sortedScores = tagScores
                .OrderByDescending(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return Results.Ok(new TagSummaryDto(
                sortedScores,
                now
            ));
        })
        .WithName("GetTagSummary")
        .WithSummary("Get tag summary with decay")
        .WithDescription("Returns user's tag preferences with time-based decay applied");
    }
}
