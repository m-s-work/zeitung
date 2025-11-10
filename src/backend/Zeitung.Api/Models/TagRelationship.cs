namespace Zeitung.Api.Models;

/// <summary>
/// Represents the relationship and co-occurrence count between two tags.
/// Used for calculating tag similarity.
/// </summary>
public class TagRelationship
{
    public int Id { get; set; }
    
    public int Tag1Id { get; set; }
    
    public Tag Tag1 { get; set; } = null!;
    
    public int Tag2Id { get; set; }
    
    public Tag Tag2 { get; set; } = null!;
    
    /// <summary>
    /// Number of times these tags appear together
    /// </summary>
    public int CoOccurrenceCount { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
