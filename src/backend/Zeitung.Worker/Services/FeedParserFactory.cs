using Zeitung.Worker.Models;

namespace Zeitung.Worker.Services;

/// <summary>
/// Factory for selecting the appropriate feed parser based on feed configuration.
/// </summary>
public interface IFeedParserFactory
{
    /// <summary>
    /// Gets the appropriate parser for the given feed.
    /// </summary>
    /// <param name="feed">Feed to parse</param>
    /// <returns>Parser that can handle the feed</returns>
    IFeedParser GetParser(RssFeed feed);
}

public class FeedParserFactory : IFeedParserFactory
{
    private readonly IEnumerable<IFeedParser> _parsers;
    private readonly ILogger<FeedParserFactory> _logger;

    public FeedParserFactory(
        IEnumerable<IFeedParser> parsers,
        ILogger<FeedParserFactory> logger)
    {
        _parsers = parsers;
        _logger = logger;
    }

    public IFeedParser GetParser(RssFeed feed)
    {
        ArgumentNullException.ThrowIfNull(feed);

        var parser = _parsers.FirstOrDefault(p => p.CanHandle(feed));
        
        if (parser == null)
        {
            throw new InvalidOperationException(
                $"No parser found for feed '{feed.Name}' with type '{feed.Type}'");
        }

        _logger.LogDebug("Selected parser {ParserType} for feed {FeedName}", 
            parser.GetType().Name, feed.Name);

        return parser;
    }
}
