namespace Zeitung.Api.Models;

/// <summary>
/// Represents a content tag
/// </summary>
public record Tag
{
    /// <summary>
    /// Unique identifier for the tag
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Tag name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Tag category (e.g., "topic", "sentiment", "entity")
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// How many times this tag has been used
    /// </summary>
    public int UsageCount { get; init; }

    /// <summary>
    /// When the tag was created
    /// </summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// User's preference for a tag
/// </summary>
public record UserTagPreference
{
    /// <summary>
    /// Tag ID
    /// </summary>
    public required string TagId { get; init; }

    /// <summary>
    /// Tag name
    /// </summary>
    public required string TagName { get; init; }

    /// <summary>
    /// User ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Preference type
    /// </summary>
    public required TagPreferenceType PreferenceType { get; init; }

    /// <summary>
    /// Preference strength (0-1, with decay applied)
    /// </summary>
    public double Strength { get; init; }

    /// <summary>
    /// When the preference was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the preference was last updated
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Type of tag preference
/// </summary>
public enum TagPreferenceType
{
    /// <summary>
    /// User explicitly marked as interested
    /// </summary>
    Explicit,

    /// <summary>
    /// User explicitly marked as ignored
    /// </summary>
    Ignored,

    /// <summary>
    /// Inferred from clicked articles
    /// </summary>
    InferredFromClick,

    /// <summary>
    /// Inferred from liked articles
    /// </summary>
    InferredFromLike
}

/// <summary>
/// Request to vote on a tag
/// </summary>
public record TagVoteRequest
{
    /// <summary>
    /// Tag ID or name
    /// </summary>
    public required string Tag { get; init; }

    /// <summary>
    /// Vote type
    /// </summary>
    public required TagVoteType VoteType { get; init; }
}

/// <summary>
/// Type of tag vote
/// </summary>
public enum TagVoteType
{
    /// <summary>
    /// Mark tag as interesting
    /// </summary>
    Interested,

    /// <summary>
    /// Mark tag as ignored
    /// </summary>
    Ignored,

    /// <summary>
    /// Remove any preference
    /// </summary>
    Neutral
}

/// <summary>
/// Request to vote on multiple article tags
/// </summary>
public record ArticleTagVotesRequest
{
    /// <summary>
    /// Article ID
    /// </summary>
    public required string ArticleId { get; init; }

    /// <summary>
    /// Tag votes
    /// </summary>
    public required List<TagVoteRequest> Votes { get; init; }
}
