using System.Xml;
using Zeitung.Worker.Models;

namespace Zeitung.Worker.Services;

/// <summary>
/// Parser for RDF (RSS 1.0) format feeds.
/// </summary>
public interface IRdfFeedParser : IFeedParser
{
    Task<List<Article>> ParseRdfFeedAsync(string xmlContent, RssFeed feed, CancellationToken cancellationToken = default);
}

public class RdfFeedParser : IRdfFeedParser
{
    private readonly ILogger<RdfFeedParser> _logger;

    public RdfFeedParser(ILogger<RdfFeedParser> logger)
    {
        _logger = logger;
    }

    public bool CanHandle(RssFeed feed)
    {
        return feed.Type.Equals("rdf", StringComparison.OrdinalIgnoreCase);
    }

    public Task<List<Article>> ParseFeedAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("RdfFeedParser should be called through RssFeedParser which detects RDF format");
    }

    public async Task<List<Article>> ParseRdfFeedAsync(string xmlContent, RssFeed feed, CancellationToken cancellationToken = default)
    {
        var articles = new List<Article>();
        
        using var stringReader = new StringReader(xmlContent);
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
