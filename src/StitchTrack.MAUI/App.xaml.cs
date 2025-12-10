using Microsoft.EntityFrameworkCore;
using StitchTrack.Infrastructure.Data;
using StitchTrack.MAUI.Data;

namespace StitchTrack.MAUI;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App(AppDbContext dbContext)
    {
        InitializeComponent();

        // Initialize database with detailed logging
        InitializeDatabaseAsync(dbContext).GetAwaiter().GetResult();

        MainPage = new AppShell();
    }

    private static async Task InitializeDatabaseAsync(AppDbContext dbContext)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== DATABASE INITIALIZATION STARTED ===");
            System.Diagnostics.Debug.WriteLine($"Database Path: {DatabaseConfig.DatabasePath}");
            System.Diagnostics.Debug.WriteLine($"App Data Directory: {FileSystem.AppDataDirectory}");

            await DbInitializer.InitializeAsync(dbContext).ConfigureAwait(false);

            System.Diagnostics.Debug.WriteLine("=== DATABASE INITIALIZATION COMPLETED ===");

            // Verify database file exists
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
            // Database-specific errors (CA1031: Catch specific exception)
            System.Diagnostics.Debug.WriteLine($"❌ DATABASE UPDATE ERROR: {dbEx.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {dbEx.StackTrace}");
            if (dbEx.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
            }
            throw; // Re-throw to prevent app from running with broken database
        }
        catch (InvalidOperationException ioEx)
        {
            // Configuration errors (CA1031: Catch specific exception)
            System.Diagnostics.Debug.WriteLine($"❌ CONFIGURATION ERROR: {ioEx.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ioEx.StackTrace}");
            throw; // Re-throw to prevent app from running with broken database
        }
    }
}
