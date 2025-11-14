namespace Zeitung.Worker.Models;

public class RssFeed
{
    public required string Url { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    
    /// <summary>
    /// Feed type: rss (default), rdf, html5
    /// </summary>
    public string Type { get; init; } = "rss";
    
    /// <summary>
    /// HTML5 parsing configuration (required when Type is "html5")
    /// </summary>
    public HtmlFeedConfig? HtmlConfig { get; init; }
    
    /// <summary>
    /// URL patterns for generating multiple feeds from one configuration.
    /// When specified, each pattern will create a separate feed with placeholders replaced.
    /// Example: ["bau", "ittk", "3d-druck"] with URL template "https://example.com/{category}/"
    /// </summary>
    public List<string>? UrlPatterns { get; init; }
}

