using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeitung.Core.Models;

/// <summary>
/// Represents an RSS feed that can be personal or globally approved.
/// </summary>
public class Feed
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Url { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this feed is approved for global feed list
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// User who added this feed (null for system feeds)
    /// </summary>
    public int? AddedByUserId { get; set; }

    public User? AddedByUser { get; set; }

    /// <summary>
    /// When the feed was promoted to global list
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastFetchedAt { get; set; }

    /// <summary>
    /// Navigation property: Users who subscribed to this feed
    /// </summary>
    public ICollection<UserFeed> UserFeeds { get; set; } = new List<UserFeed>();

    /// <summary>
    /// Navigation property: Articles from this feed
    /// </summary>
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
