using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StitchTrack.Infrastructure.Data;

/// <summary>
/// Factory for creating DbContext at design-time for migrations.
/// Only used by EF Core tools, not in actual app.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use a temporary SQLite database for migrations
        optionsBuilder.UseSqlite("Data Source=designtime.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}
