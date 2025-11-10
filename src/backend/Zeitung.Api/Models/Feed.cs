namespace Zeitung.Api.Models;

/// <summary>
/// Represents an RSS feed source
/// </summary>
public record Feed
{
    /// <summary>
    /// Unique identifier for the feed
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Feed title
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Feed URL
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Feed description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Whether this feed is globally promoted
    /// </summary>
    public bool IsGlobal { get; init; }

    /// <summary>
    /// User ID who owns this feed (null for global feeds)
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// When the feed was added
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Last time the feed was fetched
    /// </summary>
    public DateTime? LastFetchedAt { get; init; }
}

/// <summary>
/// Request to create a new feed
/// </summary>
public record CreateFeedRequest
{
    /// <summary>
    /// Feed URL
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Optional title override
    /// </summary>
    public string? Title { get; init; }
}

/// <summary>
/// Request to promote a feed to global
/// </summary>
public record PromoteFeedRequest
{
    /// <summary>
    /// Feed ID to promote
    /// </summary>
    public required string FeedId { get; init; }
}
