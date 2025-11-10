using System.ComponentModel.DataAnnotations;

namespace Zeitung.Worker.Models;

public class UserEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsActive { get; set; } = true;
}
