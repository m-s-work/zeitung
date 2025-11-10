using Microsoft.EntityFrameworkCore;
using Zeitung.Api.Models;

namespace Zeitung.Api.Data;

/// <summary>
/// Database context for Zeitung RSS Feed Reader.
/// </summary>
public class ZeitungDbContext : DbContext
{
    public ZeitungDbContext(DbContextOptions<ZeitungDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Feed> Feeds { get; set; }
    public DbSet<UserFeed> UserFeeds { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ArticleTag> ArticleTags { get; set; }
    public DbSet<TagRelationship> TagRelationships { get; set; }
    public DbSet<UserTag> UserTags { get; set; }
    public DbSet<Vote> Votes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Feed entity
        modelBuilder.Entity<Feed>(entity =>
        {
            entity.HasIndex(e => e.Url).IsUnique();
            entity.HasOne(e => e.AddedByUser)
                .WithMany()
                .HasForeignKey(e => e.AddedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // UserFeed entity
        modelBuilder.Entity<UserFeed>(entity =>
        {
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

        // Article entity
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasIndex(e => e.Link).IsUnique();
            entity.HasOne(e => e.Feed)
                .WithMany(f => f.Articles)
                .HasForeignKey(e => e.FeedId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Tag entity
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ArticleTag entity
        modelBuilder.Entity<ArticleTag>(entity =>
        {
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

        // TagRelationship entity
        modelBuilder.Entity<TagRelationship>(entity =>
        {
            entity.HasIndex(e => new { e.Tag1Id, e.Tag2Id }).IsUnique();
            entity.HasOne(e => e.Tag1)
                .WithMany(t => t.RelatedTags)
                .HasForeignKey(e => e.Tag1Id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag2)
                .WithMany()
                .HasForeignKey(e => e.Tag2Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserTag entity
        modelBuilder.Entity<UserTag>(entity =>
        {
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
    }
}
