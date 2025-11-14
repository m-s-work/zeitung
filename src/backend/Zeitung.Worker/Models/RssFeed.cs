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
    /// Can be a simple list of strings (slugs) or a dictionary mapping slugs to display names.
    /// Example: ["bau", "ittk", "3d-druck"] or { "bau": "Construction", "ittk": "IT & Communication" }
    /// </summary>
    public List<string>? UrlPatterns { get; init; }
    
    /// <summary>
    /// Optional display names for URL patterns. Maps pattern slug to human-readable name.
    /// Example: { "3d-druck": "3D Printing", "ittk": "IT & Communication" }
    /// </summary>
    public Dictionary<string, string>? PatternNames { get; init; }
}

