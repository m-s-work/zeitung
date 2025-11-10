using Microsoft.EntityFrameworkCore;
using Zeitung.Worker.Data;

namespace Zeitung.Worker.Services;

public interface ITagRepository
{
    Task<List<string>> GetAllTagsAsync(CancellationToken cancellationToken = default);
    Task SaveArticleTagsAsync(int articleId, List<string> tags, CancellationToken cancellationToken = default);
}

public class InMemoryTagRepository : ITagRepository
{
    private readonly HashSet<string> _tags = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<List<string>> GetAllTagsAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return _tags.ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveArticleTagsAsync(int articleId, List<string> tags, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            foreach (var tag in tags)
            {
                _tags.Add(tag);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
