using System.ServiceModel.Syndication;
using System.Xml;
using Zeitung.Worker.Models;

namespace Zeitung.Worker.Services;

public interface IRssFeedParser : IFeedParser
{
}

public class RssFeedParser : IRssFeedParser
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RssFeedParser> _logger;
    private readonly IRdfFeedParser _rdfFeedParser;

    public RssFeedParser(
        IHttpClientFactory httpClientFactory, 
        ILogger<RssFeedParser> logger,
        IRdfFeedParser rdfFeedParser)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _rdfFeedParser = rdfFeedParser;
    }

    public bool CanHandle(RssFeed feed)
    {
        // Handle RSS/Atom feeds (default type) and RDF feeds
        return string.IsNullOrEmpty(feed.Type) 
               || feed.Type.Equals("rss", StringComparison.OrdinalIgnoreCase)
               || feed.Type.Equals("atom", StringComparison.OrdinalIgnoreCase)
               || feed.Type.Equals("rdf", StringComparison.OrdinalIgnoreCase);
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
            // Delegate to RdfFeedParser
            _logger.LogWarning("Feed '{FeedName}' appears to be RDF/RSS 1.0 format. Delegating to RDF parser.", feed.Name);
            
            return await _rdfFeedParser.ParseRdfFeedAsync(content, feed, cancellationToken);
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
}

