using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zeitung.Worker.Models;

public class ArticleEntity
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

    [Required]
    [MaxLength(500)]
    public required string FeedSource { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ArticleTagEntity> ArticleTags { get; set; } = new List<ArticleTagEntity>();
}
