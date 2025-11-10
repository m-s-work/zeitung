namespace Zeitung.Api.Models;

/// <summary>
/// Many-to-many relationship between Articles and Tags.
/// </summary>
public class ArticleTag
{
    public int Id { get; set; }
    
    public int ArticleId { get; set; }
    
    public Article Article { get; set; } = null!;
    
    public int TagId { get; set; }
    
    public Tag Tag { get; set; } = null!;
    
    /// <summary>
    /// Confidence/probability score from tagging (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
