using Zeitung.Worker.Models;

namespace Zeitung.Worker.Strategies;

public class FeedBasedTaggingStrategy : ITaggingStrategy
{
    public Task<List<string>> GenerateTagsAsync(Article article, CancellationToken cancellationToken = default)
    {
        // Extract tags from article categories and metadata
        var tags = new List<string>(article.Categories);
        
        // Extract keywords from title and description
        var words = article.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Concat(article.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(w => w.Length > 4) // Only words longer than 4 characters
            .Select(w => w.ToLowerInvariant().Trim(',', '.', '!', '?', ':', ';'))
            .Distinct()
            .Take(5);
        
        tags.AddRange(words);
        
        return Task.FromResult(tags.Distinct().ToList());
    }
}
