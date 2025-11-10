namespace Zeitung.Api.Models;

/// <summary>
/// Represents an article from an RSS feed.
/// </summary>
public class Article
{
    public int Id { get; set; }
    
    public required string Title { get; set; }
    
    public required string Link { get; set; }
    
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
