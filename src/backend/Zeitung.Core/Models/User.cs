using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeitung.Core.Models;

/// <summary>
/// Represents a user in the system.
/// Initially supports single user, but prepared for multi-user scenarios.
/// </summary>
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Username { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Email { get; set; }

    /// <summary>
    /// User role for authorization (User, Mod, SuperMod, Admin)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastSyncAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last login timestamp for authentication tracking
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Indicates if the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

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
