using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Interfaces;

/// <summary>
/// Repository interface for PatternFile entity.
/// Handles persistence of PDF pattern files attached to projects.
/// </summary>
public interface IPatternFileRepository
{
    /// <summary>
    /// Adds a new pattern file record linked to a project.
    /// Call SaveChangesAsync to persist.
    /// </summary>
    Task AddAsync(PatternFile patternFile);

    /// <summary>
    /// Returns all pattern files for a given project.
    /// </summary>
    Task<IEnumerable<PatternFile>> GetByProjectIdAsync(Guid projectId);

    /// <summary>
    /// Deletes a pattern file record.
    /// The caller is responsible for deleting the physical file separately.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync();
}
