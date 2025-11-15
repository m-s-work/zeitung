using Zeitung.Worker.Models;

namespace Zeitung.Worker.Services;

/// <summary>
/// Common interface for all feed parsers (RSS, RDF, HTML5, etc.)
/// </summary>
public interface IFeedParser
{
    /// <summary>
    /// Parses a feed from the given URL and returns a list of articles.
    /// </summary>
    /// <param name="feed">Feed configuration with URL and name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of parsed articles</returns>
    Task<List<Article>> ParseFeedAsync(RssFeed feed, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if this parser can handle the given feed based on feed configuration or content.
    /// </summary>
    /// <param name="feed">Feed to check</param>
    /// <returns>True if this parser can handle the feed</returns>
    bool CanHandle(RssFeed feed);
}
