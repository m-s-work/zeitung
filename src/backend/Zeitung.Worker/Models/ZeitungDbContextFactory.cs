using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zeitung.Worker.Models;

public class ZeitungDbContextFactory : IDesignTimeDbContextFactory<ZeitungDbContext>
{
    public ZeitungDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ZeitungDbContext>();
        
        // Use a default connection string for migrations
        optionsBuilder.UseNpgsql("Host=localhost;Database=zeitung;Username=zeitung;Password=zeitung");

        return new ZeitungDbContext(optionsBuilder.Options);
    }
}
