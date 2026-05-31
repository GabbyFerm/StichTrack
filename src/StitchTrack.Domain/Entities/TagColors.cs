// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
namespace StitchTrack.Domain.Entities;

/// <summary>
/// Fixed color palette for project tag chips.
/// All colors are mid-dark enough for white text to be readable,
/// and they stand out on both the light cream and dark grey page backgrounds.
/// Color is assigned by position: index % Palette.Length — so the same
/// tag list always renders the same colors.
/// </summary>
public static class TagColors
{
    public static readonly string[] Palette =
    [
        "#C0713D", // Terracotta  — warm, brand-adjacent
        "#6A9E72", // Sage green
        "#5B89A8", // Steel blue
        "#9B72A8", // Soft purple
        "#A85B6A", // Dusty rose
        "#6A9E9E"  // Muted teal
    ];

    /// <summary>
    /// Returns the color hex for a tag at the given position in the list.
    /// Wraps around if there are more tags than palette entries.
    /// </summary>
    public static string GetColor(int index) => Palette[index % Palette.Length];
}
