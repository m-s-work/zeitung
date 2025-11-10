using Zeitung.Worker.Models;

namespace Zeitung.Worker.Strategies;

public class MockTaggingStrategy : ITaggingStrategy
{
    public Task<List<string>> GenerateTagsAsync(Article article, CancellationToken cancellationToken = default)
    {
        // Return predictable mock tags for testing
        var tags = new List<string> { "mock-tag-1", "mock-tag-2", "test" };
        return Task.FromResult(tags);
    }
}
