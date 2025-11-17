using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zeitung.Api.DTOs;
using Zeitung.Core.Context;

namespace Zeitung.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly ZeitungDbContext _db;

        public UserController(ZeitungDbContext db)
        {
            _db = db;
        }

        /// <summary>Returns information about the current user.</summary>
        [HttpGet("current")]
        public async Task<ActionResult<UserDto>> GetCurrentUser([FromQuery] int userId)
        {
            var user = await _db.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userDto = new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                user.LastSyncAt
            );

            return Ok(userDto);
        }

        /// <summary>Returns sync state for SPA/mobile apps and updates last sync time.</summary>
        [HttpGet("sync")]
        public async Task<ActionResult<SyncStateDto>> GetSyncState([FromQuery] int userId)
        {
            var user = await _db.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Count unread articles (articles published after last sync)
            var unreadCount = await _db.Articles
                .Where(a => a.PublishedDate > user.LastSyncAt)
                .Where(a => a.Feed.IsApproved || a.Feed.UserFeeds.Any(uf => uf.UserId == userId))
                .CountAsync();

            // Count new feeds (feeds added after last sync)
            var newFeedCount = await _db.Feeds
                .Where(f => f.CreatedAt > user.LastSyncAt)
                .Where(f => f.IsApproved || f.UserFeeds.Any(uf => uf.UserId == userId))
                .CountAsync();

            // Update last sync time
            user.LastSyncAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var syncState = new SyncStateDto(
                user.LastSyncAt,
                unreadCount,
                newFeedCount
            );

            return Ok(syncState);
        }
    }
}