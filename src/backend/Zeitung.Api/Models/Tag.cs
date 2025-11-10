namespace Zeitung.Api.Models;

/// <summary>
/// Represents a tag that can be associated with articles.
/// </summary>
public class Tag
{
    public int Id { get; set; }
    
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
    /// Navigation property: Tag relationships (co-occurrence)
    /// </summary>
    public ICollection<TagRelationship> RelatedTags { get; set; } = new List<TagRelationship>();
}
