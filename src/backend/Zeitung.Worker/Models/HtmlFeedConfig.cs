namespace Zeitung.Worker.Models;

/// <summary>
/// Configuration for HTML5 feed parsing using CSS selectors.
/// Similar to html2rss config format.
/// </summary>
public class HtmlFeedConfig
{
    /// <summary>
    /// CSS selector for the items/articles container
    /// </summary>
    public required string ItemsSelector { get; init; }
    
    /// <summary>
    /// Selector configuration for title extraction
    /// </summary>
    public required SelectorConfig Title { get; init; }
    
    /// <summary>
    /// Selector configuration for link extraction
    /// </summary>
    public required SelectorConfig Link { get; init; }
    
    /// <summary>
    /// Optional selector configuration for description extraction
    /// </summary>
    public SelectorConfig? Description { get; init; }
    
    /// <summary>
    /// Optional selector configuration for published date extraction
    /// </summary>
    public SelectorConfig? PublishedAt { get; init; }
    
    /// <summary>
    /// Optional selector configuration for category extraction
    /// </summary>
    public SelectorConfig? Category { get; init; }
}

/// <summary>
/// Configuration for extracting data using CSS selectors
/// </summary>
public class SelectorConfig
{
    /// <summary>
    /// CSS selector to locate the element
    /// </summary>
    public required string Selector { get; init; }
    
    /// <summary>
    /// Extractor type: text (default), href, src, datetime, or attribute name
    /// </summary>
    public string Extractor { get; init; } = "text";
    
    /// <summary>
    /// Optional attribute name when extractor is "attribute"
    /// </summary>
    public string? Attribute { get; init; }
}
