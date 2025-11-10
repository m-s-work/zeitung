using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeitung.Core.Models;

/// <summary>
/// Represents an article from an RSS feed.
/// </summary>
public class Article
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Link { get; set; }

    [Required]
    public required string Description { get; set; }

    public DateTime PublishedDate { get; set; }

    public int FeedId { get; set; }
    
    public Feed Feed { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: Tags associated with this article
    /// </summary>
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();

    /// <summary>
    /// Navigation property: Votes on this article
    /// </summary>
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
