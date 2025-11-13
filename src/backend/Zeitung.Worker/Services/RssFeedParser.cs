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
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(feed.Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // Try to parse with SyndicationFeed first
        try
        {
            using var stringReader = new StringReader(content);
            using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            var syndicationFeed = SyndicationFeed.Load(xmlReader);
            return ParseSyndicationFeed(syndicationFeed, feed);
        }
        catch (XmlException ex) when (ex.Message.Contains("RDF"))
        {
            // RDF format (RSS 1.0) not directly supported by SyndicationFeed
            // Attempt to parse as generic XML
            _logger.LogWarning("Feed '{FeedName}' appears to be RDF/RSS 1.0 format. Attempting manual parsing.", feed.Name);
            
            return await ParseRdfFeedAsync(content, feed, cancellationToken);
        }
    }

    private List<Article> ParseSyndicationFeed(SyndicationFeed syndicationFeed, RssFeed feed)
    {
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

    private async Task<List<Article>> ParseRdfFeedAsync(string content, RssFeed feed, CancellationToken cancellationToken)
    {
        var articles = new List<Article>();
        
        using var stringReader = new StringReader(content);
        using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, Async = true });
        
        while (await xmlReader.ReadAsync())
        {
            if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.LocalName == "item")
            {
                var article = await ParseRdfItemAsync(xmlReader.ReadSubtree(), feed);
                if (article != null)
                {
                    articles.Add(article);
                }
            }
        }

        _logger.LogInformation("Parsed {Count} articles from RDF feed: {FeedName}", articles.Count, feed.Name);
        return articles;
    }

    private async Task<Article?> ParseRdfItemAsync(XmlReader itemReader, RssFeed feed)
    {
        string? title = null;
        string? link = null;
        string? description = null;
        DateTime publishedDate = default;
        var categories = new List<string>();

        while (await itemReader.ReadAsync())
        {
            if (itemReader.NodeType == XmlNodeType.Element)
            {
                switch (itemReader.LocalName)
                {
                    case "title":
                        title = await itemReader.ReadElementContentAsStringAsync();
                        break;
                    case "link":
                        link = await itemReader.ReadElementContentAsStringAsync();
                        break;
                    case "description":
                        description = await itemReader.ReadElementContentAsStringAsync();
                        break;
                    case "date":
                    case "pubDate":
                        var dateStr = await itemReader.ReadElementContentAsStringAsync();
                        if (DateTime.TryParse(dateStr, out var pubDate))
                        {
                            publishedDate = pubDate;
                        }
                        break;
                    case "category":
                    case "subject":
                        var category = await itemReader.ReadElementContentAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(category))
                        {
                            categories.Add(category);
                        }
                        break;
                }
            }
        }

        // Only return article if it has at least a title or link
        if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(link))
        {
            return new Article
            {
                Title = title ?? "No Title",
                Link = link ?? "",
                Description = description ?? "",
                PublishedDate = publishedDate,
                Categories = categories,
                FeedSource = feed.Name
            };
        }

        return null;
    }
}

