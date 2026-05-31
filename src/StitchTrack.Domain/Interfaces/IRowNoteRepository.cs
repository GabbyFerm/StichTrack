// Copyright (c) 2026 Gabriella Frank Ferm / Frank Ferm Design. All rights reserved.
using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Interfaces;

/// <summary>
/// Repository interface for RowNote operations.
/// Row notes are in-the-moment reminders attached to a specific row number,
/// e.g. "decrease here" at row 12. Separate from project notes which are
/// set in the project form.
/// </summary>
public interface IRowNoteRepository
{
    /// <summary>
    /// Returns all notes for a project, ordered by row number ascending.
    /// </summary>
    Task<IEnumerable<RowNote>> GetByProjectIdAsync(Guid projectId);

    Task AddAsync(RowNote note);

    /// <summary>
    /// Hard deletes a note by ID. Uses ExecuteDeleteAsync — no SaveChanges needed.
    /// </summary>
    Task DeleteAsync(Guid id);

    Task<int> SaveChangesAsync();
}
