using System.ComponentModel.DataAnnotations;

namespace Zeitung.Worker.Models;

public class RefreshTokenEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    public UserEntity User { get; set; } = null!;
    
    [Required]
    [MaxLength(512)]
    public string Token { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    public DateTime? RevokedAt { get; set; }
    
    public bool IsRevoked => RevokedAt.HasValue;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public bool IsActive => !IsRevoked && !IsExpired;
}
