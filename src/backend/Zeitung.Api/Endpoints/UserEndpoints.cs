using Microsoft.EntityFrameworkCore;
using Zeitung.Api.Data;
using Zeitung.Api.DTOs;

namespace Zeitung.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        // GET /api/users/current - Get current user info
        group.MapGet("/current", async (int userId, ZeitungDbContext db) =>
        {
            var user = await db.Users.FindAsync(userId);
            
            if (user == null)
            {
                return Results.NotFound(new { message = "User not found" });
            }

            var userDto = new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                user.LastSyncAt
            );

            return Results.Ok(userDto);
        })
        .WithName("GetCurrentUser")
        .WithSummary("Get current user")
        .WithDescription("Returns information about the current user");

        // GET /api/users/sync - Get sync state for SPA/mobile
        group.MapGet("/sync", async (int userId, ZeitungDbContext db) =>
        {
            var user = await db.Users.FindAsync(userId);
            
            if (user == null)
            {
                return Results.NotFound(new { message = "User not found" });
            }

            // Count unread articles (articles published after last sync)
            var unreadCount = await db.Articles
                .Where(a => a.PublishedDate > user.LastSyncAt)
                .Where(a => a.Feed.IsApproved || a.Feed.UserFeeds.Any(uf => uf.UserId == userId))
                .CountAsync();

            // Count new feeds (feeds added after last sync)
            var newFeedCount = await db.Feeds
                .Where(f => f.CreatedAt > user.LastSyncAt)
                .Where(f => f.IsApproved || f.UserFeeds.Any(uf => uf.UserId == userId))
                .CountAsync();

            // Update last sync time
            user.LastSyncAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            var syncState = new SyncStateDto(
                user.LastSyncAt,
                unreadCount,
                newFeedCount
            );

            return Results.Ok(syncState);
        })
        .WithName("GetSyncState")
        .WithSummary("Get sync state")
        .WithDescription("Returns sync state for SPA/mobile apps and updates last sync time");
    }
}
