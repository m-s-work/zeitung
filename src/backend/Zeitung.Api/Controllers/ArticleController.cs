using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zeitung.Core.Models;
using Zeitung.Api.DTOs;
using Zeitung.Core.Context;

namespace Zeitung.Api.Controllers
{
    [ApiController]
    [Route("api/articles")]
    public class ArticleController : ControllerBase
    {
        private readonly ZeitungDbContext _db;

        public ArticleController(ZeitungDbContext db)
        {
            _db = db;
        }

        /// <summary>Get articles (paginated) with optional filtering by feed or tag.</summary>
        [HttpGet]
        public async Task<ActionResult> GetArticles([FromQuery] int? userId, [FromQuery] int? feedId, [FromQuery] int? tagId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _db.Articles
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

            return Ok(new
            {
                Articles = articleDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        /// <summary>Get detailed information about a specific article.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleDto>> GetArticle(int id, [FromQuery] int? userId)
        {
            var article = await _db.Articles
                .Include(a => a.Feed)
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound(new { message = "Article not found" });
            }

            int? userVote = null;
            if (userId.HasValue)
            {
                var vote = await _db.Votes
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

            return Ok(articleDto);
        }

        /// <summary>Vote on an article (upvote or downvote).</summary>
        [HttpPost("{id}/vote")]
        public async Task<ActionResult> VoteArticle(int id, [FromBody] VoteDto dto, [FromQuery] int userId)
        {
            if (dto.Value != 1 && dto.Value != -1)
            {
                return BadRequest(new { message = "Vote value must be 1 or -1" });
            }

            var article = await _db.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound(new { message = "Article not found" });
            }

            var existingVote = await _db.Votes
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ArticleId == id);

            if (existingVote != null)
            {
                if (existingVote.Value == dto.Value)
                {
                    _db.Votes.Remove(existingVote);
                }
                else
                {
                    existingVote.Value = dto.Value;
                }
            }
            else
            {
                _db.Votes.Add(new Vote
                {
                    UserId = userId,
                    ArticleId = id,
                    Value = dto.Value
                });
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Vote recorded" });
        }

        /// <summary>Like an article and update user's tag preferences.</summary>
        [HttpPost("{id}/like")]
        public async Task<ActionResult> LikeArticle(int id, [FromQuery] int userId)
        {
            var article = await _db.Articles
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound(new { message = "Article not found" });
            }

            foreach (var articleTag in article.ArticleTags)
            {
                var userTag = await _db.UserTags
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
                    _db.UserTags.Add(new UserTag
                    {
                        UserId = userId,
                        TagId = articleTag.TagId,
                        InteractionType = InteractionType.Liked,
                        Score = articleTag.Confidence,
                        InteractionCount = 1
                    });
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Article liked, tag preferences updated" });
        }

        /// <summary>Track an article click and update user's tag preferences with lower weight.</summary>
        [HttpPost("{id}/click")]
        public async Task<ActionResult> ClickArticle(int id, [FromQuery] int userId)
        {
            var article = await _db.Articles
                .Include(a => a.ArticleTags)
                    .ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound(new { message = "Article not found" });
            }

            foreach (var articleTag in article.ArticleTags)
            {
                var userTag = await _db.UserTags
                    .FirstOrDefaultAsync(ut =>
                        ut.UserId == userId &&
                        ut.TagId == articleTag.TagId &&
                        ut.InteractionType == InteractionType.Clicked);

                if (userTag != null)
                {
                    userTag.Score += articleTag.Confidence * 0.5;
                    userTag.InteractionCount++;
                    userTag.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _db.UserTags.Add(new UserTag
                    {
                        UserId = userId,
                        TagId = articleTag.TagId,
                        InteractionType = InteractionType.Clicked,
                        Score = articleTag.Confidence * 0.5,
                        InteractionCount = 1
                    });
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Article click tracked" });
        }
    }
}