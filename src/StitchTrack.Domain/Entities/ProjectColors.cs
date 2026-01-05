namespace StitchTrack.Domain.Entities;

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
        "#14B8A6", // Teal
        "#F59E0B"  // Amber
    };

    public static string GetRandomColor()
    {
        var random = new Random();
        return Palette[random.Next(Palette.Length)];
    }
}
