using Microsoft.EntityFrameworkCore;
using StitchTrack.Infrastructure.Data;

namespace StitchTrack.MAUI;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App(AppDbContext dbContext)
    {
        InitializeComponent();

        // Force light mode during development
        // UserAppTheme = AppTheme.Light;

        MainPage = new AppShell();
    }

    private static async Task ForceRecreateDatabaseAsync(AppDbContext dbContext)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("======================================");
            System.Diagnostics.Debug.WriteLine("🔥 FORCE DATABASE RECREATION START");
            System.Diagnostics.Debug.WriteLine("======================================");

            // Get connection string for logging
            var connectionString = dbContext.Database.GetConnectionString();
            System.Diagnostics.Debug.WriteLine($"📍 Connection String: {connectionString}");

            // Step 1: DELETE existing database
            System.Diagnostics.Debug.WriteLine("\n🗑️  Step 1: Deleting existing database...");
            var deleted = await dbContext.Database.EnsureDeletedAsync().ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine($"   Database deleted: {deleted}");

            // Step 2: Check for pending migrations
            System.Diagnostics.Debug.WriteLine("\n📋 Step 2: Checking for migrations...");
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync().ConfigureAwait(false);
            var pendingList = pendingMigrations.ToList();

            if (pendingList.Any())
            {
                System.Diagnostics.Debug.WriteLine($"   Found {pendingList.Count} migrations:");
                foreach (var migration in pendingList)
                {
                    System.Diagnostics.Debug.WriteLine($"   - {migration}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("   ⚠️  NO MIGRATIONS FOUND!");
                System.Diagnostics.Debug.WriteLine("   This means migrations aren't included in the build!");
            }

            // Step 3: Apply migrations
            System.Diagnostics.Debug.WriteLine("\n📦 Step 3: Applying migrations...");
            await dbContext.Database.MigrateAsync().ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("   ✅ MigrateAsync completed");

            // Step 4: Verify database was created
            System.Diagnostics.Debug.WriteLine("\n🔍 Step 4: Verifying database...");
            var canConnect = await dbContext.Database.CanConnectAsync().ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine($"   Can connect: {canConnect}");

            if (!canConnect)
            {
                System.Diagnostics.Debug.WriteLine("   ❌ CANNOT CONNECT TO DATABASE!");
                throw new Exception("Failed to connect to database after migration");
            }

            // Step 5: Check for Projects table
            System.Diagnostics.Debug.WriteLine("\n📊 Step 5: Checking Projects table...");
            try
            {
                var projectCount = await dbContext.Projects.CountAsync().ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine($"   ✅ Projects table exists! Count: {projectCount}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"   ❌ Projects table does NOT exist!");
                System.Diagnostics.Debug.WriteLine($"   Error: {ex.Message}");
                throw;
            }

            // Step 6: List applied migrations
            System.Diagnostics.Debug.WriteLine("\n✅ Step 6: Migrations applied:");
            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync().ConfigureAwait(false);
            foreach (var migration in appliedMigrations)
            {
                System.Diagnostics.Debug.WriteLine($"   ✓ {migration}");
            }

            System.Diagnostics.Debug.WriteLine("\n======================================");
            System.Diagnostics.Debug.WriteLine("✅ DATABASE RECREATION SUCCESSFUL");
            System.Diagnostics.Debug.WriteLine("======================================\n");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("\n======================================");
            System.Diagnostics.Debug.WriteLine("❌ DATABASE RECREATION FAILED");
            System.Diagnostics.Debug.WriteLine("======================================");
            System.Diagnostics.Debug.WriteLine($"Exception Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace:\n{ex.StackTrace}");

            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"\nInner Exception: {ex.InnerException.Message}");
            }

            System.Diagnostics.Debug.WriteLine("======================================\n");
            throw;
        }
    }
}
