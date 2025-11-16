using Microsoft.EntityFrameworkCore;
using Zeitung.Core.Models;

namespace Zeitung.Core.Context;

/// <summary>
/// Database context for Zeitung RSS Feed Reader.
/// Unified context for both Worker and API projects.
/// </summary>
public class ZeitungDbContext : DbContext
{
    public ZeitungDbContext(DbContextOptions<ZeitungDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<ArticleTag> ArticleTags { get; set; } = null!;
    public DbSet<TagCoOccurrence> TagCoOccurrences { get; set; } = null!;

    // User management entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Feed> Feeds { get; set; } = null!;
    public DbSet<UserFeed> UserFeeds { get; set; } = null!;
    public DbSet<UserTag> UserTags { get; set; } = null!;
    public DbSet<Vote> Votes { get; set; } = null!;
    
    // Authentication entities
    public DbSet<MagicLink> MagicLinks { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Feed entity
        modelBuilder.Entity<Feed>(entity =>
        {
            entity.ToTable("Feeds");
            entity.HasIndex(e => e.Url).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasOne(e => e.AddedByUser)
                .WithMany()
                .HasForeignKey(e => e.AddedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Article entity
        modelBuilder.Entity<Article>(entity =>
        {
            entity.ToTable("Articles");
            entity.HasIndex(e => e.Link).IsUnique();
            entity.HasIndex(e => e.PublishedDate);
            entity.HasIndex(e => e.FeedId);
            entity.HasOne(e => e.Feed)
                .WithMany(f => f.Articles)
                .HasForeignKey(e => e.FeedId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Tag entity
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ArticleTag entity
        modelBuilder.Entity<ArticleTag>(entity =>
        {
            entity.ToTable("ArticleTags");
            entity.HasIndex(e => new { e.ArticleId, e.TagId }).IsUnique();
            
            entity.HasOne(e => e.Article)
                .WithMany(a => a.ArticleTags)
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TagCoOccurrence entity
        modelBuilder.Entity<TagCoOccurrence>(entity =>
        {
            entity.ToTable("TagCoOccurrences");
            entity.HasIndex(e => new { e.Tag1Id, e.Tag2Id }).IsUnique();
            
            entity.HasOne(e => e.Tag1)
                .WithMany(t => t.TagCoOccurrences1)
                .HasForeignKey(e => e.Tag1Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Tag2)
                .WithMany(t => t.TagCoOccurrences2)
                .HasForeignKey(e => e.Tag2Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserFeed entity
        modelBuilder.Entity<UserFeed>(entity =>
        {
            entity.ToTable("UserFeeds");
            entity.HasIndex(e => new { e.UserId, e.FeedId }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserFeeds)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Feed)
                .WithMany(f => f.UserFeeds)
                .HasForeignKey(e => e.FeedId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserTag entity
        modelBuilder.Entity<UserTag>(entity =>
        {
            entity.ToTable("UserTags");
            entity.HasIndex(e => new { e.UserId, e.TagId, e.InteractionType }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserTags)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.UserTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Vote entity
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("Votes");
            entity.HasIndex(e => new { e.UserId, e.ArticleId, e.FeedId }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Article)
                .WithMany(a => a.Votes)
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Feed)
                .WithMany()
                .HasForeignKey(e => e.FeedId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // MagicLink entity
        modelBuilder.Entity<MagicLink>(entity =>
        {
            entity.ToTable("MagicLinks");
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.Email);
        });
        
        // RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
