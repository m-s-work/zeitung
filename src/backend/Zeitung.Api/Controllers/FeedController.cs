using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zeitung.Core.Models;
using Zeitung.Api.DTOs;

namespace Zeitung.Api.Controllers
{
    [ApiController]
    [Route("api/feeds")]
    public class FeedController : ControllerBase
    {
        private readonly ZeitungDbContext _db;

        public FeedController(ZeitungDbContext db)
        {
            _db = db;
        }

        /// <summary>Get all feeds (global + user's personal).</summary>
        [HttpGet]
        public async Task<ActionResult> GetFeeds([FromQuery] int? userId)
        {
            var feeds = await _db.Feeds
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

            return Ok(feedDtos);
        }

        /// <summary>Add feed to personal list.</summary>
        [HttpPost]
        public async Task<ActionResult> CreateFeed([FromBody] CreateFeedDto dto, [FromQuery] int userId)
        {
            // Check if feed already exists
            var existingFeed = await _db.Feeds.FirstOrDefaultAsync(f => f.Url == dto.Url);

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
                _db.Feeds.Add(feed);
                await _db.SaveChangesAsync();
            }

            // Check if user already subscribed
            var existingSubscription = await _db.UserFeeds
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.FeedId == feed.Id);

            if (existingSubscription != null)
            {
                return Conflict(new { message = "Already subscribed to this feed" });
            }

            var userFeed = new UserFeed
            {
                UserId = userId,
                FeedId = feed.Id
            };
            _db.UserFeeds.Add(userFeed);
            await _db.SaveChangesAsync();

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

            return Created($"/api/feeds/{feed.Id}", feedDto);
        }

        /// <summary>Remove feed from personal list.</summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFeed(int id, [FromQuery] int userId)
        {
            var userFeed = await _db.UserFeeds
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.FeedId == id);

            if (userFeed == null)
            {
                return NotFound(new { message = "Feed subscription not found" });
            }

            _db.UserFeeds.Remove(userFeed);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Promote feed to global list.</summary>
        [HttpPost("{id}/promote")]
        public async Task<ActionResult> PromoteFeed(int id)
        {
            var feed = await _db.Feeds.FindAsync(id);

            if (feed == null)
            {
                return NotFound(new { message = "Feed not found" });
            }

            if (feed.IsApproved)
            {
                return Conflict(new { message = "Feed is already approved" });
            }

            feed.IsApproved = true;
            feed.ApprovedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Feed promoted to global list" });
        }

        /// <summary>Get feed recommendations based on user's tag preferences.</summary>
        [HttpGet("recommendations")]
        public async Task<ActionResult> GetFeedRecommendations([FromQuery] int userId)
        {
            // Get user's tag preferences
            var userTags = await _db.UserTags
                .Where(ut => ut.UserId == userId && ut.InteractionType != InteractionType.Ignored)
                .Include(ut => ut.Tag)
                .OrderByDescending(ut => ut.Score)
                .Take(20)
                .ToListAsync();

            if (!userTags.Any())
            {
                // Return popular feeds if user has no preferences
                var popularFeeds = await _db.Feeds
                    .Where(f => f.IsApproved)
                    .Take(10)
                    .ToListAsync();

                return Ok(popularFeeds.Select(f => new FeedRecommendationDto(
                    f.Id,
                    f.Url,
                    f.Name,
                    f.Description,
                    0.5,
                    new System.Collections.Generic.List<string>()
                )).ToList());
            }

            var userTagIds = userTags.Select(ut => ut.TagId).ToList();

            // Find feeds with articles that match user's interests
            var recommendedFeeds = await _db.Feeds
                .Where(f => !_db.UserFeeds.Any(uf => uf.UserId == userId && uf.FeedId == f.Id))
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
                Math.Min(x.Score / 10.0, 1.0),
                x.RelevantTags
            )).ToList();

            return Ok(recommendations);
        }
    }
}