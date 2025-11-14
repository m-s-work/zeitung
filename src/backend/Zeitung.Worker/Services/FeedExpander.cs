using Zeitung.Worker.Models;

namespace Zeitung.Worker.Services;

/// <summary>
/// Service for expanding feed configurations with URL patterns into multiple feeds.
/// </summary>
public static class FeedExpander
{
    /// <summary>
    /// Expands a feed configuration with URL patterns into multiple feed instances.
    /// If no patterns are specified, returns the original feed.
    /// </summary>
    /// <param name="feed">Feed configuration to expand</param>
    /// <returns>List of expanded feed configurations</returns>
    public static List<RssFeed> ExpandFeed(RssFeed feed)
    {
        if (feed.UrlPatterns == null || feed.UrlPatterns.Count == 0)
        {
            return new List<RssFeed> { feed };
        }

        var expandedFeeds = new List<RssFeed>();

        foreach (var pattern in feed.UrlPatterns)
        {
            var expandedUrl = feed.Url.Replace("{pattern}", pattern);
            var expandedName = feed.Name.Replace("{pattern}", pattern);
            var expandedDescription = feed.Description?.Replace("{pattern}", pattern);

            var expandedFeed = new RssFeed
            {
                Url = expandedUrl,
                Name = expandedName,
                Description = expandedDescription,
                Type = feed.Type,
                HtmlConfig = feed.HtmlConfig
            };

            expandedFeeds.Add(expandedFeed);
        }

        return expandedFeeds;
    }

    /// <summary>
    /// Expands a list of feed configurations, processing any with URL patterns.
    /// </summary>
    /// <param name="feeds">List of feed configurations</param>
    /// <returns>Expanded list of feeds with patterns resolved</returns>
    public static List<RssFeed> ExpandFeeds(IEnumerable<RssFeed> feeds)
    {
        var expandedFeeds = new List<RssFeed>();

        foreach (var feed in feeds)
        {
            expandedFeeds.AddRange(ExpandFeed(feed));
        }

        return expandedFeeds;
    }
}
