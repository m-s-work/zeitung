using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeitung.Worker.Models;

public class TagEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ArticleTagEntity> ArticleTags { get; set; } = new List<ArticleTagEntity>();
    public ICollection<TagCoOccurrenceEntity> TagCoOccurrences1 { get; set; } = new List<TagCoOccurrenceEntity>();
    public ICollection<TagCoOccurrenceEntity> TagCoOccurrences2 { get; set; } = new List<TagCoOccurrenceEntity>();
}
