using System.Security.Cryptography;

namespace StitchTrack.Domain.Entities;

/// <summary>
/// Predefined color palette for project cards and display elements.
/// (For tag colors, see TagColors class.)
/// Provides a random selection from the palette for new projects.
/// </summary>
public static class ProjectColors
{
    public static readonly string[] Palette = new[]
    {
        "#6B7280", // Gray
        "#8B5CF6", // Purple
        "#10B981", // Green
        "#3B82F6", // Blue
        "#EAB308", // Yellow
        "#F97316", // Orange
        "#EF4444", // Red
        "#EC4899", // Pink
        "#14B8A6" // Teal
    };

    /// <summary>
    /// Returns a random color from the palette using cryptographically secure RNG.
    /// </summary>
    public static string GetRandomColor()
    {
        var randomIndex = RandomNumberGenerator.GetInt32(0, Palette.Length);
        return Palette[randomIndex];
    }
}
