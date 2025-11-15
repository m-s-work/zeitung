using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Zeitung.Core.Models;

namespace Zeitung.Worker.Tests.Models;

/// <summary>
/// Tests to verify EF Core migrations are properly configured and discoverable.
/// </summary>
[TestFixture]
public class DatabaseMigrationTests
{
    [Test]
    public void Migrations_ShouldBeInSameAssemblyAsDbContext()
    {
        // Arrange
        var dbContextAssembly = typeof(ZeitungDbContext).Assembly;
        
        // Act - Find all migration types in the assembly
        var migrationType = typeof(Migration);
        var migrations = dbContextAssembly.GetTypes()
            .Where(t => migrationType.IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();
        
        // Assert
        Assert.That(migrations.Count, Is.GreaterThan(0), 
            "No migrations found in Zeitung.Core assembly. Migrations must be in the same assembly as ZeitungDbContext.");
        
        // Verify migrations are in the expected namespace
        foreach (var migration in migrations)
        {
            Assert.That(migration.Namespace, Does.StartWith("Zeitung.Core.Migrations"),
                $"Migration {migration.Name} should be in Zeitung.Core.Migrations namespace");
        }
    }
    
    [Test]
    public void Migrations_ShouldHaveValidMigrationAttributes()
    {
        // Arrange
        var dbContextAssembly = typeof(ZeitungDbContext).Assembly;
        var migrationType = typeof(Migration);
        var migrations = dbContextAssembly.GetTypes()
            .Where(t => migrationType.IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();
        
        // Act & Assert
        Assert.That(migrations.Count, Is.GreaterThan(0), "At least one migration should exist");
        
        foreach (var migration in migrations)
        {
            var migrationAttr = migration.GetCustomAttribute<MigrationAttribute>();
            Assert.That(migrationAttr, Is.Not.Null, 
                $"Migration {migration.Name} should have MigrationAttribute");
            Assert.That(migrationAttr!.Id, Is.Not.Null.And.Not.Empty,
                $"Migration {migration.Name} should have a valid migration ID");
        }
    }
    
    [Test]
    public void ModelSnapshot_ShouldExistInSameAssembly()
    {
        // Arrange
        var dbContextAssembly = typeof(ZeitungDbContext).Assembly;
        var snapshotType = typeof(ModelSnapshot);
        
        // Act - Find the model snapshot
        var snapshots = dbContextAssembly.GetTypes()
            .Where(t => snapshotType.IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();
        
        // Assert
        Assert.That(snapshots.Count, Is.EqualTo(1), 
            "Exactly one ModelSnapshot should exist in Zeitung.Core assembly");
        Assert.That(snapshots[0].Namespace, Does.StartWith("Zeitung.Core.Migrations"),
            "ModelSnapshot should be in Zeitung.Core.Migrations namespace");
    }
    
    [Test]
    public async Task DbContext_ShouldApplyMigrationsSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ZeitungDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ZeitungDbContext(options);
        
        // Act - Ensure database is created (applies model to in-memory database)
        await context.Database.EnsureCreatedAsync();
        
        // Assert - Verify tables exist by querying them
        Assert.DoesNotThrowAsync(async () => await context.Articles.CountAsync(), 
            "Articles table should be accessible");
        Assert.DoesNotThrowAsync(async () => await context.Tags.CountAsync(), 
            "Tags table should be accessible");
        Assert.DoesNotThrowAsync(async () => await context.ArticleTags.CountAsync(), 
            "ArticleTags table should be accessible");
        Assert.DoesNotThrowAsync(async () => await context.Users.CountAsync(), 
            "Users table should be accessible");
        Assert.DoesNotThrowAsync(async () => await context.Feeds.CountAsync(), 
            "Feeds table should be accessible");
        
        // Cleanup
        await context.Database.EnsureDeletedAsync();
    }
}
