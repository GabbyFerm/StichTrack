namespace StitchTrack.MAUI.Data;

/// <summary>
/// Provides platform-specific database paths and connection strings.
/// </summary>
public static class DatabaseConfig
{
    // Name of the SQLite database file
    public const string DatabaseFilename = "stitchtrack.db3";

    // Gets the full path to the database file for the current platform
    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    // Gets the SQLite connection string with recommended settings
    public static string ConnectionString =>
        $"Data Source={DatabasePath};Cache=Shared;";
}
