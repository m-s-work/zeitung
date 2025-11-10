namespace Zeitung.Api.Models;

/// <summary>
/// Represents a user's subscription to a feed.
/// </summary>
public class UserFeed
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    public int FeedId { get; set; }
    
    public Feed Feed { get; set; } = null!;
    
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
}
