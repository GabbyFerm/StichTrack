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

            // Create database if it doesn't exist and apply all migrations
            await context.Database.MigrateAsync().ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine("✅ Migrations applied successfully");

            // Count existing projects
            var projectCount = await context.Projects.CountAsync().ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine($"📊 Current projects in database: {projectCount}");

            // Create test project if database is empty
            if (projectCount == 0)
            {
                System.Diagnostics.Debug.WriteLine("➕ Creating test project...");

                var testProject = Project.CreateProject("Test Project from Database");
                context.Projects.Add(testProject);
                await context.SaveChangesAsync().ConfigureAwait(false);

                System.Diagnostics.Debug.WriteLine($"✅ Test project created: {testProject.Name} (ID: {testProject.Id})");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ DATABASE INIT FAILED: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Exception Type: {ex.GetType().Name}");

            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }

            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw; // Re-throw to let caller handle it
        }
    }
}
