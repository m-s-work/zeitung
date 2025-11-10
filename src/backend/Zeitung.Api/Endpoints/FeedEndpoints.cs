using Zeitung.Api.Models;

namespace Zeitung.Api.Endpoints;

/// <summary>
/// Feed management endpoints
/// </summary>
public static class FeedEndpoints
{
    public static void MapFeedEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/feeds")
            .WithTags("Feeds")
            .WithOpenApi();

        // Get all feeds for current user
        group.MapGet("/", GetFeeds)
            .WithName("GetFeeds")
            .WithSummary("Get all feeds")
            .WithDescription("Returns all feeds accessible by the current user (personal + global)");

        // Get a specific feed
        group.MapGet("/{id}", GetFeed)
            .WithName("GetFeed")
            .WithSummary("Get a specific feed")
            .WithDescription("Returns details of a specific feed");

        // Create a new feed
        group.MapPost("/", CreateFeed)
            .WithName("CreateFeed")
            .WithSummary("Create a new feed")
            .WithDescription("Adds a new RSS feed for the current user");

        // Promote feed to global
        group.MapPost("/{id}/promote", PromoteFeed)
            .WithName("PromoteFeed")
            .WithSummary("Promote feed to global")
            .WithDescription("Promotes a user feed to the global feed list (requires permissions)");

        // Delete a feed
        group.MapDelete("/{id}", DeleteFeed)
            .WithName("DeleteFeed")
            .WithSummary("Delete a feed")
            .WithDescription("Deletes a feed (only personal feeds can be deleted by their owner)");
    }

    private static async Task<IResult> GetFeeds()
    {
        // TODO: Implement actual database query
        // For now, return mock data
        var feeds = new List<Feed>
        {
            new()
            {
                Id = "1",
                Title = "TechCrunch",
                Url = "https://techcrunch.com/feed/",
                Description = "The latest technology news",
                IsGlobal = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastFetchedAt = DateTime.UtcNow.AddHours(-1)
            },
            new()
            {
                Id = "2",
                Title = "Hacker News",
                Url = "https://news.ycombinator.com/rss",
                Description = "Hacker News RSS Feed",
                IsGlobal = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                LastFetchedAt = DateTime.UtcNow.AddMinutes(-30)
            },
            new()
            {
                Id = "3",
                Title = "My Personal Blog",
                Url = "https://myblog.example.com/feed",
                IsGlobal = false,
                UserId = "user1",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                LastFetchedAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        return Results.Ok(feeds);
    }

    private static async Task<IResult> GetFeed(string id)
    {
        // TODO: Implement actual database query
        if (id == "1")
        {
            return Results.Ok(new Feed
            {
                Id = "1",
                Title = "TechCrunch",
                Url = "https://techcrunch.com/feed/",
                Description = "The latest technology news",
                IsGlobal = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastFetchedAt = DateTime.UtcNow.AddHours(-1)
            });
        }

        return Results.NotFound(new { message = "Feed not found" });
    }

    private static async Task<IResult> CreateFeed(CreateFeedRequest request)
    {
        // TODO: Implement actual feed creation
        // Should validate URL, fetch feed info, and store in database
        var feed = new Feed
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title ?? "New Feed",
            Url = request.Url,
            IsGlobal = false,
            UserId = "user1", // TODO: Get from authentication
            CreatedAt = DateTime.UtcNow
        };

        return Results.Created($"/api/feeds/{feed.Id}", feed);
    }

    private static async Task<IResult> PromoteFeed(string id)
    {
        // TODO: Implement actual promotion
        // Should check user permissions and update feed
        return Results.Ok(new { message = "Feed promoted to global", feedId = id });
    }

    private static async Task<IResult> DeleteFeed(string id)
    {
        // TODO: Implement actual deletion
        // Should check ownership and delete from database
        return Results.NoContent();
    }
}
