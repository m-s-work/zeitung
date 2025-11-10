using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeitung.Core.Models;

/// <summary>
/// Represents a vote on an article or feed.
/// </summary>
public class Vote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>
    /// Article being voted on (null if voting on feed)
    /// </summary>
    public int? ArticleId { get; set; }

    public Article? Article { get; set; }

    /// <summary>
    /// Feed being voted on (null if voting on article)
    /// </summary>
    public int? FeedId { get; set; }

    public Feed? Feed { get; set; }

    /// <summary>
    /// Vote value: 1 for upvote, -1 for downvote
    /// </summary>
    public int Value { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
