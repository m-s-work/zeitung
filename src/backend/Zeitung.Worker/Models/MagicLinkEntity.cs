using System.ComponentModel.DataAnnotations;

namespace Zeitung.Worker.Models;

public class MagicLinkEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = null!;
    
    [Required]
    [MaxLength(512)]
    public string Token { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    public DateTime? UsedAt { get; set; }
    
    public bool IsUsed => UsedAt.HasValue;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public bool IsValid => !IsUsed && !IsExpired;
}
