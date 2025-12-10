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
    /// Gets all non-archived projects for the current user.
    /// </summary>
    Task<IEnumerable<Project>> GetActiveProjectsAsync(Guid? userId = null);

    /// <summary>
    /// Gets all archived projects for the current user.
    /// </summary>
    Task<IEnumerable<Project>> GetArchivedProjectsAsync(Guid? userId = null);

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    Task UpdateAsync(Project project);

    /// <summary>
    /// Deletes a project (soft delete by setting IsArchived = true).
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync();
}
