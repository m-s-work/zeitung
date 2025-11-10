using Zeitung.Worker.Models;

namespace Zeitung.Worker.Strategies;

public interface ITaggingStrategy
{
    Task<List<string>> GenerateTagsAsync(Article article, CancellationToken cancellationToken = default);
}
