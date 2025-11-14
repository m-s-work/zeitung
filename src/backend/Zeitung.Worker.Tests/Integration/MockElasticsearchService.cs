using Zeitung.Worker.Models;
using Zeitung.Worker.Services;

namespace Zeitung.Worker.Tests.Integration;

/// <summary>
/// Mock Elasticsearch service for testing that doesn't require actual Elasticsearch
/// </summary>
internal class MockElasticsearchService : IElasticsearchService
{
    public Task IndexArticleAsync(Article article, int articleId, CancellationToken cancellationToken = default)
    {
        // No-op for testing - we don't need actual Elasticsearch indexing in unit tests
        return Task.CompletedTask;
    }
}
