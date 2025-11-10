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

            // Apply interaction-count-based decay
            // As users add new interests, older interests decay proportionally
            // This ensures the system adapts to changing interests naturally
            var tagScores = new Dictionary<string, double>();
            
            // Calculate total interaction count to determine decay
            var totalInteractionCount = userTags
                .Where(ut => ut.InteractionType != InteractionType.Ignored)
                .Sum(ut => ut.InteractionCount);
            
            // Decay constant: how much influence total interactions have on decay
            // Higher value = more aggressive decay as new interests are added
            const double decayConstant = 0.1;
            
            foreach (var userTag in userTags)
            {
                // Skip ignored tags
                if (userTag.InteractionType == InteractionType.Ignored)
                {
                    continue;
                }
                
                // Calculate decay factor based on relative interaction count
                // Tags with fewer interactions decay more as total interactions increase
                var relativeInteractionCount = totalInteractionCount > 0 
                    ? (double)userTag.InteractionCount / totalInteractionCount 
                    : 1.0;
                
                // Apply exponential decay: score * e^(-k * (1 - relative))
                // Tags with higher relative interaction counts decay less
                var decayFactor = Math.Exp(-decayConstant * totalInteractionCount * (1 - relativeInteractionCount));
                var decayedScore = userTag.Score * decayFactor;
                
                // Skip very low scores
                if (decayedScore < 0.1)
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
                DateTime.UtcNow
            ));
        })
        .WithName("GetTagSummary")
        .WithSummary("Get tag summary with decay")
        .WithDescription("Returns user's tag preferences with interaction-count-based decay applied");
    }
}
