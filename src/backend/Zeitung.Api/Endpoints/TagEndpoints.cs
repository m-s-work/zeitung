using Zeitung.Api.Models;

namespace Zeitung.Api.Endpoints;

/// <summary>
/// Tag and preference endpoints
/// </summary>
public static class TagEndpoints
{
    public static void MapTagEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tags")
            .WithTags("Tags")
            .WithOpenApi();

        // Get all tags
        group.MapGet("/", GetTags)
            .WithName("GetTags")
            .WithSummary("Get all tags")
            .WithDescription("Returns all available tags with usage statistics");

        // Get user's tag preferences
        group.MapGet("/preferences", GetUserPreferences)
            .WithName("GetUserTagPreferences")
            .WithSummary("Get user tag preferences")
            .WithDescription("Returns user's tag preferences with current strength values");

        // Vote on a tag
        group.MapPost("/vote", VoteOnTag)
            .WithName("VoteOnTag")
            .WithSummary("Vote on a tag")
            .WithDescription("Records user's preference for a tag (interested, ignored, or neutral)");

        // Vote on multiple article tags
        group.MapPost("/articles/{articleId}/votes", VoteOnArticleTags)
            .WithName("VoteOnArticleTags")
            .WithSummary("Vote on article tags")
            .WithDescription("Records user's preferences for multiple tags associated with an article");
    }

    private static async Task<IResult> GetTags(int? limit, string? category)
    {
        // TODO: Implement actual database query
        // For now, return mock data
        var tags = new List<Tag>
        {
            new()
            {
                Id = "1",
                Name = "AI",
                Category = "topic",
                UsageCount = 150,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = "2",
                Name = "Software Development",
                Category = "topic",
                UsageCount = 200,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new()
            {
                Id = "3",
                Name = "Technology",
                Category = "topic",
                UsageCount = 300,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new()
            {
                Id = "4",
                Name = "Rust",
                Category = "topic",
                UsageCount = 80,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = "5",
                Name = "Kubernetes",
                Category = "topic",
                UsageCount = 120,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            }
        };

        var result = tags.AsQueryable();
        
        if (!string.IsNullOrEmpty(category))
        {
            result = result.Where(t => t.Category == category);
        }

        if (limit.HasValue)
        {
            result = result.Take(limit.Value);
        }

        return Results.Ok(result.ToList());
    }

    private static async Task<IResult> GetUserPreferences()
    {
        // TODO: Implement actual database query
        // For now, return mock data
        var preferences = new List<UserTagPreference>
        {
            new()
            {
                TagId = "1",
                TagName = "AI",
                UserId = "user1",
                PreferenceType = TagPreferenceType.Explicit,
                Strength = 0.95,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                TagId = "2",
                TagName = "Software Development",
                UserId = "user1",
                PreferenceType = TagPreferenceType.InferredFromLike,
                Strength = 0.82,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new()
            {
                TagId = "6",
                TagName = "Politics",
                UserId = "user1",
                PreferenceType = TagPreferenceType.Ignored,
                Strength = 0.0,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        return Results.Ok(preferences);
    }

    private static async Task<IResult> VoteOnTag(TagVoteRequest request)
    {
        // TODO: Implement actual voting logic
        // Should update user preferences and recalculate recommendations
        return Results.Ok(new { message = "Tag vote recorded", tag = request.Tag, voteType = request.VoteType });
    }

    private static async Task<IResult> VoteOnArticleTags(string articleId, ArticleTagVotesRequest request)
    {
        // TODO: Implement actual voting logic for multiple tags
        return Results.Ok(new { message = "Article tag votes recorded", articleId, voteCount = request.Votes.Count });
    }
}
