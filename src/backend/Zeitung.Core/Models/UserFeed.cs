using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeitung.Core.Models;

/// <summary>
/// Represents a user's subscription to a feed.
/// </summary>
public class UserFeed
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int FeedId { get; set; }

    public Feed Feed { get; set; } = null!;

    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
}
