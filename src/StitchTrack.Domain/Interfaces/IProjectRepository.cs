using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Interfaces;

/// <summary>
/// Repository interface for Project entity operations.
/// Provides methods for CRUD operations and querying projects.
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Adds a new project to the database.
    /// </summary>
    Task AddAsync(Project project);

    /// <summary>
    /// Gets a project by its unique identifier.
    /// </summary>
    Task<Project?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a project by ID without loading CounterHistory entries.
    /// Use for the counter page where history tracking causes EF state conflicts.
    /// </summary>
    Task<Project?> GetByIdWithoutHistoryAsync(Guid id);

    /// <summary>
    /// Gets all non-archived projects for the current user.
    /// </summary>
    Task<IEnumerable<Project>> GetActiveProjectsAsync(Guid? userId = null);

    /// <summary>
    /// Gets all archived projects for the current user.
    /// </summary>
    Task<IEnumerable<Project>> GetArchivedProjectsAsync(Guid? userId = null);

    /// <summary>
    /// Gets all projects with sessions included, for export.
    /// </summary>
    Task<List<Project>> GetAllForExportAsync(bool includeArchived = false, Guid? userId = null);

    /// <summary>
    /// Replaces all tags for a project with the provided list.
    /// Assigns colors by position (index % TagColors.Palette.Length).
    /// Caller must call SaveChangesAsync() afterwards.
    /// </summary>
    Task UpdateTagsAsync(Guid projectId, IReadOnlyList<string> tagNames);

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    Task UpdateAsync(Project project);
    /// <summary>
    /// Updates only the Count and UpdatedAt fields of a project.
    /// </summary>
    Task UpdateCountAsync(Guid projectId, int newCount, DateTime updatedAt);

    /// <summary>
    /// Soft delete — sets IsArchived = true. Project remains in the database.
    /// Use this when the user wants to "hide" a project but keep it recoverable.
    /// </summary>
    Task ArchiveAsync(Guid id);

    /// <summary>
    /// Hard delete — permanently removes the project row from the database.
    /// This cannot be undone. Use only after explicit user confirmation.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync();
}
