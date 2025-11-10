using Microsoft.EntityFrameworkCore;

namespace Zeitung.Worker.Models;

public class ZeitungDbContext : DbContext
{
    public ZeitungDbContext(DbContextOptions<ZeitungDbContext> options)
        : base(options)
    {
    }

    public DbSet<ArticleEntity> Articles { get; set; } = null!;
    public DbSet<TagEntity> Tags { get; set; } = null!;
    public DbSet<ArticleTagEntity> ArticleTags { get; set; } = null!;
    public DbSet<TagCoOccurrenceEntity> TagCoOccurrences { get; set; } = null!;
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; } = null!;
    public DbSet<MagicLinkEntity> MagicLinks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ArticleEntity
        modelBuilder.Entity<ArticleEntity>(entity =>
        {
            entity.ToTable("Articles");
            entity.HasIndex(e => e.Link).IsUnique();
            entity.HasIndex(e => e.PublishedDate);
            entity.HasIndex(e => e.FeedSource);
        });

        // Configure TagEntity
        modelBuilder.Entity<TagEntity>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure ArticleTagEntity
        modelBuilder.Entity<ArticleTagEntity>(entity =>
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

        // Configure TagCoOccurrenceEntity
        modelBuilder.Entity<TagCoOccurrenceEntity>(entity =>
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

        // Configure UserEntity
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure RefreshTokenEntity
        modelBuilder.Entity<RefreshTokenEntity>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MagicLinkEntity
        modelBuilder.Entity<MagicLinkEntity>(entity =>
        {
            entity.ToTable("MagicLinks");
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.Email);
        });
    }
}
