namespace Zeitung.Api.Models;

/// <summary>
/// Represents an article from a feed
/// </summary>
public record Article
{
    /// <summary>
    /// Unique identifier for the article
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Article title
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Article URL
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Article content/summary
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Article author
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Feed this article belongs to
    /// </summary>
    public required string FeedId { get; init; }

    /// <summary>
    /// Feed title
    /// </summary>
    public string? FeedTitle { get; init; }

    /// <summary>
    /// Publication date
    /// </summary>
    public DateTime PublishedAt { get; init; }

    /// <summary>
    /// When article was added to system
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Tags associated with this article
    /// </summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>
    /// Recommendation score for current user
    /// </summary>
    public double? RecommendationScore { get; init; }
}

/// <summary>
/// Represents a recommended article with metadata
/// </summary>
public record RecommendedArticle
{
    /// <summary>
    /// The article
    /// </summary>
    public required Article Article { get; init; }

    /// <summary>
    /// Recommendation score (0-1)
    /// </summary>
    public required double Score { get; init; }

    /// <summary>
    /// Why this article was recommended
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// User's interaction with this article
    /// </summary>
    public UserArticleInteraction? Interaction { get; init; }
}

/// <summary>
/// User interaction with an article
/// </summary>
public record UserArticleInteraction
{
    /// <summary>
    /// Article ID
    /// </summary>
    public required string ArticleId { get; init; }

    /// <summary>
    /// User ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Whether user liked the recommendation
    /// </summary>
    public bool? LikedRecommendation { get; init; }

    /// <summary>
    /// Whether user opened the article
    /// </summary>
    public bool Opened { get; init; }

    /// <summary>
    /// Time when article was opened
    /// </summary>
    public DateTime? OpenedAt { get; init; }

    /// <summary>
    /// Time user spent reading (in seconds)
    /// </summary>
    public int? ReadTimeSeconds { get; init; }

    /// <summary>
    /// User's rating (1-5 stars)
    /// </summary>
    public int? Rating { get; init; }

    /// <summary>
    /// When the interaction was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the interaction was last updated
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Request to record recommendation feedback
/// </summary>
public record RecommendationFeedbackRequest
{
    /// <summary>
    /// Article ID
    /// </summary>
    public required string ArticleId { get; init; }

    /// <summary>
    /// Whether user liked the recommendation
    /// </summary>
    public required bool Liked { get; init; }
}

/// <summary>
/// Request to record article read time
/// </summary>
public record ArticleReadTimeRequest
{
    /// <summary>
    /// Article ID
    /// </summary>
    public required string ArticleId { get; init; }

    /// <summary>
    /// Time spent reading (in seconds)
    /// </summary>
    public required int ReadTimeSeconds { get; init; }
}

/// <summary>
/// Request to rate an article
/// </summary>
public record ArticleRatingRequest
{
    /// <summary>
    /// Article ID
    /// </summary>
    public required string ArticleId { get; init; }

    /// <summary>
    /// Rating (1-5 stars)
    /// </summary>
    public required int Rating { get; init; }
}
