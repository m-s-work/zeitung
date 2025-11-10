namespace Zeitung.Api.Models;

/// <summary>
/// Represents a user's interaction with a tag.
/// Different interaction types are tracked separately for better recommendation.
/// </summary>
public class UserTag
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    public int TagId { get; set; }
    
    public Tag Tag { get; set; } = null!;
    
    /// <summary>
    /// Type of interaction: Explicit, Ignored, Clicked, Liked
    /// </summary>
    public required string InteractionType { get; set; }
    
    /// <summary>
    /// Current interest score (decays over time)
    /// </summary>
    public double Score { get; set; }
    
    /// <summary>
    /// Number of interactions of this type
    /// </summary>
    public int InteractionCount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Interaction types for user-tag relationships
/// </summary>
public static class InteractionType
{
    public const string Explicit = "Explicit";  // User explicitly marked as interesting
    public const string Ignored = "Ignored";    // User explicitly wants to ignore
    public const string Clicked = "Clicked";    // User clicked article with this tag
    public const string Liked = "Liked";        // User liked article with this tag
}
