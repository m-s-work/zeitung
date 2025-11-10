using System.ServiceModel.Syndication;
using System.Xml;
using Zeitung.Worker.Models;

namespace Zeitung.Worker.Services;

public interface IRssFeedParser
{
    Task<List<Article>> ParseFeedAsync(RssFeed feed, CancellationToken cancellationToken = default);
}

public class RssFeedParser : IRssFeedParser
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RssFeedParser> _logger;

    public RssFeedParser(IHttpClientFactory httpClientFactory, ILogger<RssFeedParser> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<Article>> ParseFeedAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(feed.Url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var xmlReader = XmlReader.Create(stream);
            var syndicationFeed = SyndicationFeed.Load(xmlReader);

            var articles = new List<Article>();

            foreach (var item in syndicationFeed.Items)
            {
                var article = new Article
                {
                    Title = item.Title?.Text ?? "No Title",
                    Link = item.Links.FirstOrDefault()?.Uri.ToString() ?? "",
                    Description = item.Summary?.Text ?? "",
                    PublishedDate = item.PublishDate.DateTime,
                    Categories = item.Categories.Select(c => c.Name).ToList(),
                    FeedSource = feed.Name
                };

                articles.Add(article);
            }

            _logger.LogInformation("Parsed {Count} articles from feed: {FeedName}", articles.Count, feed.Name);
            return articles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing RSS feed: {FeedUrl}", feed.Url);
            return new List<Article>();
        }
    }
}
