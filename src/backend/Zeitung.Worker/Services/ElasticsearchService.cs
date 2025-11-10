using Elastic.Clients.Elasticsearch;
using Zeitung.Worker.Models;

namespace Zeitung.Worker.Services;

public interface IElasticsearchService
{
    Task IndexArticleAsync(Article article, int articleId, CancellationToken cancellationToken = default);
}

public class ElasticsearchService : IElasticsearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchService> _logger;
    private const string IndexName = "articles";

    public ElasticsearchService(ElasticsearchClient client, ILogger<ElasticsearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task IndexArticleAsync(Article article, int articleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var articleDocument = new ArticleDocument
            {
                Id = articleId,
                Title = article.Title,
                Link = article.Link,
                Description = article.Description,
                PublishedDate = article.PublishedDate,
                FeedSource = article.FeedSource,
                Tags = article.Tags,
                IndexedAt = DateTime.UtcNow
            };

            var response = await _client.IndexAsync(articleDocument, idx => idx
                .Index(IndexName)
                .Id(articleId.ToString()), cancellationToken);

            if (response.IsValidResponse)
            {
                _logger.LogInformation("Indexed article in Elasticsearch: {Title} (ID: {Id})", article.Title, articleId);
            }
            else
            {
                _logger.LogWarning("Failed to index article in Elasticsearch: {Title}. Error: {Error}", 
                    article.Title, response.ElasticsearchServerError?.Error?.Reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing article in Elasticsearch: {Title}", article.Title);
        }
    }
}

public class ArticleDocument
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Link { get; set; }
    public required string Description { get; set; }
    public DateTime PublishedDate { get; set; }
    public required string FeedSource { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime IndexedAt { get; set; }
}
