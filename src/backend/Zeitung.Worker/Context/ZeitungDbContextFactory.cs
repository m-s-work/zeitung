using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Zeitung.Core.Context;

namespace Zeitung.Worker.Context;

/// <summary>
/// Design-time factory for creating ZeitungDbContext instances in the Worker project.
/// Used by EF Core tools for migrations.
/// </summary>
public class ZeitungDbContextFactory : IDesignTimeDbContextFactory<ZeitungDbContext>
{
    public ZeitungDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ZeitungDbContext>();
        
        // Use a default connection string for migrations
        // This doesn't need a real database connection to work - it's only used for generating migration code
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=zeitung;Username=zeitung;Password=zeitung",
            npgsqlOptions => npgsqlOptions.MigrationsAssembly("Zeitung.Worker"));

        return new ZeitungDbContext(optionsBuilder.Options);
    }
}
