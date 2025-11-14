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
}
