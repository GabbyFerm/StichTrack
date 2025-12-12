using Microsoft.EntityFrameworkCore;
using StitchTrack.Infrastructure.Data;
using StitchTrack.MAUI.Data;

namespace StitchTrack.MAUI;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App(AppDbContext dbContext)
    {
        InitializeComponent();

        // Initialize database with logging. Don't crash the UI while debugging DB issues.
        try
        {
            InitializeDatabaseAsync(dbContext).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            // TEMP: log and continue so the UI can start while we fix DB issues
            System.Diagnostics.Debug.WriteLine($"❌ DATABASE INITIALIZATION FAILED (TEMP SWALLOW): {ex}");
        }

        MainPage = new AppShell();
    }

    private static async Task InitializeDatabaseAsync(AppDbContext dbContext)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== DATABASE INITIALIZATION STARTED ===");
            System.Diagnostics.Debug.WriteLine($"Database Path: {DatabaseConfig.DatabasePath}");
            System.Diagnostics.Debug.WriteLine($"App Data Directory: {FileSystem.AppDataDirectory}");

            // Try to apply EF Core migrations (recommended)
            try
            {
                System.Diagnostics.Debug.WriteLine("Attempting to apply EF Core migrations...");
                await dbContext.Database.MigrateAsync().ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine("Migrations applied successfully.");
            }
            catch (Exception migrateEx)
            {
                // If migrations are not available or fail, fall back to EnsureCreated (development only)
                System.Diagnostics.Debug.WriteLine($"MigrateAsync failed: {migrateEx.Message}. Falling back to EnsureCreatedAsync...");
                await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine("EnsureCreatedAsync completed (fallback).");
            }

            // Run any seeding logic after migrations/creation
            await DbInitializer.InitializeAsync(dbContext).ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine("=== DATABASE INITIALIZATION COMPLETED ===");

            if (File.Exists(DatabaseConfig.DatabasePath))
            {
                var fileInfo = new FileInfo(DatabaseConfig.DatabasePath);
                System.Diagnostics.Debug.WriteLine($"✅ Database file CONFIRMED: {DatabaseConfig.DatabasePath}");
                System.Diagnostics.Debug.WriteLine($"✅ Database size: {fileInfo.Length} bytes");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ DATABASE FILE NOT FOUND: {DatabaseConfig.DatabasePath}");
            }
        }
        catch (DbUpdateException dbEx)
        {
            System.Diagnostics.Debug.WriteLine($"❌ DATABASE UPDATE ERROR: {dbEx.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {dbEx.StackTrace}");
            if (dbEx.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
            }
            throw; // rethrow so outer try/catch can log and continue (temporary)
        }
        catch (InvalidOperationException ioEx)
        {
            System.Diagnostics.Debug.WriteLine($"❌ CONFIGURATION ERROR: {ioEx.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ioEx.StackTrace}");
            throw;
        }
    }
}
