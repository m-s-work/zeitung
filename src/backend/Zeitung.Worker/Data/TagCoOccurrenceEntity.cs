using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeitung.Worker.Data;

/// <summary>
/// Tracks how often two tags appear together on articles.
/// Used to calculate tag similarity for recommendations.
/// </summary>
public class TagCoOccurrenceEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Tag1Id { get; set; }
    public TagEntity Tag1 { get; set; } = null!;

    public int Tag2Id { get; set; }
    public TagEntity Tag2 { get; set; } = null!;

    /// <summary>
    /// Number of times these two tags have appeared together on articles
    /// </summary>
    public int OccurrenceCount { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
