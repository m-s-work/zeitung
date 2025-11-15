namespace Zeitung.Worker.Models;

/// <summary>
/// Configuration options for RSS feeds
/// </summary>
public class RssFeedOptions
{
    public const string SectionName = "RssFeeds";
    
    public List<RssFeed> Feeds { get; set; } = new();
}
