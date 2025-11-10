using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeitung.Core.Models;

/// <summary>
/// Represents a tag that can be associated with articles.
/// </summary>
public class Tag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    /// <summary>
    /// Total number of times this tag has been used
    /// </summary>
    public int UsageCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: Articles tagged with this tag
    /// </summary>
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();

    /// <summary>
    /// Navigation property: User preferences for this tag
    /// </summary>
    public ICollection<UserTag> UserTags { get; set; } = new List<UserTag>();

    /// <summary>
    /// Navigation property: Tag co-occurrences (relationships)
    /// </summary>
    public ICollection<TagCoOccurrence> TagCoOccurrences1 { get; set; } = new List<TagCoOccurrence>();
    public ICollection<TagCoOccurrence> TagCoOccurrences2 { get; set; } = new List<TagCoOccurrence>();
}
