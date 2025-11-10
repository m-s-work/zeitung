using System.ComponentModel.DataAnnotations;

namespace Zeitung.Core.Models;

/// <summary>
/// Represents a magic link token for passwordless authentication
/// </summary>
public class MagicLink
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Email { get; set; }
    
    [Required]
    [MaxLength(512)]
    public required string Token { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; }
    
    public DateTime? UsedAt { get; set; }
    
    public bool IsUsed => UsedAt.HasValue;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public bool IsValid => !IsUsed && !IsExpired;
}
