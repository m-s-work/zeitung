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
    [Route("api/tags")]
    public class TagController : ControllerBase
    {
        private readonly ZeitungDbContext _db;

        public TagController(ZeitungDbContext db)
        {
            _db = db;
        }

        /// <summary>Get paginated list of tags ordered by usage count.</summary>
        [HttpGet]
        public async Task<ActionResult> GetTags([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var totalCount = await _db.Tags.CountAsync();

            var tags = await _db.Tags
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

            return Ok(new
            {
                Tags = tags,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        /// <summary>Get user's tag preferences.</summary>
        [HttpGet("user")]
        public async Task<ActionResult> GetUserTags([FromQuery] int userId)
        {
            var userTags = await _db.UserTags
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

            return Ok(preferences);
        }

        /// <summary>Mark tags as explicitly interesting to the user.</summary>
        [HttpPost("interest")]
        public async Task<ActionResult> MarkTagsInterest([FromBody] TagInterestDto dto, [FromQuery] int userId)
        {
            foreach (var tagId in dto.TagIds)
            {
                var tag = await _db.Tags.FindAsync(tagId);
                if (tag == null)
                {
                    continue;
                }

                var userTag = await _db.UserTags
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
                    _db.UserTags.Add(new UserTag
                    {
                        UserId = userId,
                        TagId = tagId,
                        InteractionType = InteractionType.Explicit,
                        Score = 1.0,
                        InteractionCount = 1
                    });
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Tag interests updated" });
        }

        /// <summary>Mark tags as ignored by the user.</summary>
        [HttpPost("ignore")]
        public async Task<ActionResult> IgnoreTags([FromBody] TagInterestDto dto, [FromQuery] int userId)
        {
            foreach (var tagId in dto.TagIds)
            {
                var tag = await _db.Tags.FindAsync(tagId);
                if (tag == null)
                {
                    continue;
                }

                var userTag = await _db.UserTags
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
                    _db.UserTags.Add(new UserTag
                    {
                        UserId = userId,
                        TagId = tagId,
                        InteractionType = InteractionType.Ignored,
                        Score = -10.0,
                        InteractionCount = 1
                    });
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Tags marked as ignored" });
        }

        /// <summary>Get tag summary with interaction-count-based decay applied.</summary>
        [HttpGet("summary")]
        public async Task<ActionResult<TagSummaryDto>> GetTagSummary([FromQuery] int userId)
        {
            var userTags = await _db.UserTags
                .Where(ut => ut.UserId == userId)
                .Include(ut => ut.Tag)
                .ToListAsync();

            // Apply interaction-count-based decay
            var tagScores = new System.Collections.Generic.Dictionary<string, double>();

            var totalInteractionCount = userTags
                .Where(ut => ut.InteractionType != InteractionType.Ignored)
                .Sum(ut => ut.InteractionCount);

            const double decayConstant = 0.1;

            foreach (var userTag in userTags)
            {
                if (userTag.InteractionType == InteractionType.Ignored)
                {
                    continue;
                }

                var relativeInteractionCount = totalInteractionCount > 0
                    ? (double)userTag.InteractionCount / totalInteractionCount
                    : 1.0;

                var decayFactor = Math.Exp(-decayConstant * totalInteractionCount * (1 - relativeInteractionCount));
                var decayedScore = userTag.Score * decayFactor;

                if (decayedScore < 0.1)
                {
                    continue;
                }

                tagScores[userTag.Tag.Name] = decayedScore;
            }

            var sortedScores = tagScores
                .OrderByDescending(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return Ok(new TagSummaryDto(
                sortedScores,
                DateTime.UtcNow
            ));
        }
    }
}