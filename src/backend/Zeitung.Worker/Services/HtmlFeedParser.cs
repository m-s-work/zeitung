using AngleSharp;
using AngleSharp.Html.Dom;
using Zeitung.Worker.Models;

namespace Zeitung.Worker.Services;

/// <summary>
/// Parser for HTML5 feeds using CSS selectors.
/// Allows scraping articles from HTML pages based on configuration.
/// </summary>
public interface IHtmlFeedParser : IFeedParser
{
    Task<List<Article>> ParseHtmlFeedAsync(string htmlContent, RssFeed feed, CancellationToken cancellationToken = default);
}

public class HtmlFeedParser : IHtmlFeedParser
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HtmlFeedParser> _logger;

    public HtmlFeedParser(
        IHttpClientFactory httpClientFactory,
        ILogger<HtmlFeedParser> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool CanHandle(RssFeed feed)
    {
        return feed.Type.Equals("html5", StringComparison.OrdinalIgnoreCase) 
               && feed.HtmlConfig != null;
    }

    public async Task<List<Article>> ParseFeedAsync(RssFeed feed, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feed);
        
        if (feed.HtmlConfig == null)
        {
            throw new InvalidOperationException($"HTML config is required for HTML5 feed: {feed.Name}");
        }

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(feed.Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return await ParseHtmlFeedAsync(htmlContent, feed, cancellationToken);
    }

    public async Task<List<Article>> ParseHtmlFeedAsync(string htmlContent, RssFeed feed, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(htmlContent);
        ArgumentNullException.ThrowIfNull(feed);
        
        if (feed.HtmlConfig == null)
        {
            throw new InvalidOperationException($"HTML config is required for HTML5 feed: {feed.Name}");
        }

        var articles = new List<Article>();
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(htmlContent), cancellationToken);

        var items = document.QuerySelectorAll(feed.HtmlConfig.ItemsSelector);
        
        _logger.LogDebug("Found {Count} items using selector: {Selector}", 
            items.Length, feed.HtmlConfig.ItemsSelector);

        foreach (var item in items)
        {
            try
            {
                var article = ParseArticleFromElement(item as IHtmlElement, feed);
                if (article != null)
                {
                    articles.Add(article);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse article item from {FeedName}", feed.Name);
            }
        }

        _logger.LogInformation("Parsed {Count} articles from HTML5 feed: {FeedName}", 
            articles.Count, feed.Name);
        
        return articles;
    }

    private Article? ParseArticleFromElement(IHtmlElement? itemElement, RssFeed feed)
    {
        if (itemElement == null || feed.HtmlConfig == null)
        {
            return null;
        }

        var config = feed.HtmlConfig;
        
        // Extract title
        var title = ExtractValue(itemElement, config.Title);
        
        // Extract link
        var link = ExtractValue(itemElement, config.Link);
        
        // Only return article if it has at least a title or link
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(link))
        {
            return null;
        }

        // Extract optional fields
        var description = config.Description != null 
            ? ExtractValue(itemElement, config.Description) 
            : string.Empty;
            
        var publishedDate = config.PublishedAt != null 
            ? ParseDateTime(ExtractValue(itemElement, config.PublishedAt))
            : default;
            
        var categories = config.Category != null 
            ? ExtractMultipleValues(itemElement, config.Category)
            : new List<string>();

        return new Article
        {
            Title = title ?? "No Title",
            Link = NormalizeUrl(link ?? string.Empty, feed.Url),
            Description = description ?? string.Empty,
            PublishedDate = publishedDate,
            Categories = categories,
            FeedSource = feed.Name
        };
    }

    private string? ExtractValue(IHtmlElement element, SelectorConfig selectorConfig)
    {
        var selectedElement = element.QuerySelector(selectorConfig.Selector);
        if (selectedElement == null)
        {
            return null;
        }

        return selectorConfig.Extractor.ToLowerInvariant() switch
        {
            "text" => selectedElement.TextContent.Trim(),
            "href" => selectedElement.GetAttribute("href"),
            "src" => selectedElement.GetAttribute("src"),
            "datetime" => selectedElement.GetAttribute("datetime") ?? selectedElement.TextContent.Trim(),
            "attribute" when !string.IsNullOrWhiteSpace(selectorConfig.Attribute) 
                => selectedElement.GetAttribute(selectorConfig.Attribute),
            _ => selectedElement.TextContent.Trim()
        };
    }

    private List<string> ExtractMultipleValues(IHtmlElement element, SelectorConfig selectorConfig)
    {
        var values = new List<string>();
        var elements = element.QuerySelectorAll(selectorConfig.Selector);
        
        foreach (var selectedElement in elements)
        {
            var value = selectorConfig.Extractor.ToLowerInvariant() switch
            {
                "text" => selectedElement.TextContent.Trim(),
                "href" => selectedElement.GetAttribute("href"),
                "src" => selectedElement.GetAttribute("src"),
                "attribute" when !string.IsNullOrWhiteSpace(selectorConfig.Attribute)
                    => selectedElement.GetAttribute(selectorConfig.Attribute),
                _ => selectedElement.TextContent.Trim()
            };

            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(value);
            }
        }

        return values;
    }

    private DateTime ParseDateTime(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return default;
        }

        if (DateTime.TryParse(dateString, out var date))
        {
            return date;
        }

        _logger.LogDebug("Failed to parse date: {DateString}", dateString);
        return default;
    }

    private string NormalizeUrl(string url, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        // If URL is already absolute, return it
        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            return url;
        }

        // Try to make it absolute using the base URL
        if (Uri.TryCreate(new Uri(baseUrl), url, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        return url;
    }
}
