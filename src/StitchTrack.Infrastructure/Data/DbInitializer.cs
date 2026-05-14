using Microsoft.EntityFrameworkCore;
using StitchTrack.Domain.Entities;

namespace StitchTrack.Infrastructure.Data;

/// <summary>
/// Handles database initialization and migrations.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Ensures the database is created and all migrations are applied.
    /// Safe to call on every app startup.
    /// </summary>
    public static async Task InitializeAsync(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            System.Diagnostics.Debug.WriteLine("📦 Applying migrations...");
            await context.Database.MigrateAsync().ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("✅ Migrations applied successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ DATABASE INIT FAILED: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Exception Type: {ex.GetType().Name}");
            if (ex.InnerException != null)
                System.Diagnostics.Debug.WriteLine($"Inner: {ex.InnerException.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            throw;
        }
    }
}
