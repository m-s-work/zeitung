using Microsoft.EntityFrameworkCore;
using Zeitung.Core.Context;
using Zeitung.Core.Models;

namespace Zeitung.Worker.Services;

public class PostgresTagRepository : ITagRepository
{
    private readonly ZeitungDbContext _context;
    private readonly ILogger<PostgresTagRepository> _logger;

    public PostgresTagRepository(ZeitungDbContext context, ILogger<PostgresTagRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _context.Tags
            .Select(t => t.Name)
            .ToListAsync(cancellationToken);
        
        return tags;
    }

    public async Task SaveArticleTagsAsync(int articleId, List<string> tags, CancellationToken cancellationToken = default)
    {
        if (tags.Count == 0)
        {
            return;
        }

        // Get or create tags
        var tagEntities = new List<Tag>();
        foreach (var tagName in tags)
        {
            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name == tagName, cancellationToken);

            if (existingTag == null)
            {
                existingTag = new Tag { Name = tagName, CreatedAt = DateTime.UtcNow };
                _context.Tags.Add(existingTag);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Created new tag: {TagName}", tagName);
            }

            tagEntities.Add(existingTag);
        }

        // Create article-tag relationships
        foreach (var tag in tagEntities)
        {
            var existingArticleTag = await _context.ArticleTags
                .FirstOrDefaultAsync(at => at.ArticleId == articleId && at.TagId == tag.Id, cancellationToken);

            if (existingArticleTag == null)
            {
                _context.ArticleTags.Add(new ArticleTag
                {
                    ArticleId = articleId,
                    TagId = tag.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        // Update tag co-occurrence counts
        await UpdateTagCoOccurrencesAsync(tagEntities, cancellationToken);

        _logger.LogInformation("Saved {Count} tags for article {ArticleId}", tags.Count, articleId);
    }

    private async Task UpdateTagCoOccurrencesAsync(List<Tag> tags, CancellationToken cancellationToken)
    {
        // For each pair of tags, increment or create co-occurrence record
        for (int i = 0; i < tags.Count; i++)
        {
            for (int j = i + 1; j < tags.Count; j++)
            {
                var tag1Id = Math.Min(tags[i].Id, tags[j].Id);
                var tag2Id = Math.Max(tags[i].Id, tags[j].Id);

                var coOccurrence = await _context.TagCoOccurrences
                    .FirstOrDefaultAsync(tc => tc.Tag1Id == tag1Id && tc.Tag2Id == tag2Id, cancellationToken);

                if (coOccurrence == null)
                {
                    _context.TagCoOccurrences.Add(new TagCoOccurrence
                    {
                        Tag1Id = tag1Id,
                        Tag2Id = tag2Id,
                        OccurrenceCount = 1,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    coOccurrence.OccurrenceCount++;
                    coOccurrence.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
