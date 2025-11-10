namespace Zeitung.Api.Models;

/// <summary>
/// Represents a user in the system.
/// Initially supports single user, but prepared for multi-user scenarios.
/// </summary>
public class User
{
    public int Id { get; set; }
    
    public required string Username { get; set; }
    
    public required string Email { get; set; }
    
    /// <summary>
    /// User role for authorization (User, Mod, SuperMod, Admin)
    /// </summary>
    public required string Role { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastSyncAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property: User's personal feed subscriptions
    /// </summary>
    public ICollection<UserFeed> UserFeeds { get; set; } = new List<UserFeed>();
    
    /// <summary>
    /// Navigation property: User's tag preferences
    /// </summary>
    public ICollection<UserTag> UserTags { get; set; } = new List<UserTag>();
    
    /// <summary>
    /// Navigation property: User's votes
    /// </summary>
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
