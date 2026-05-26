using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Interfaces;

/// <summary>
/// Repository for ProjectCounter operations.
/// Manages named counters attached to a project.
/// </summary>
public interface IProjectCounterRepository
{
    /// <summary>
    /// Returns all counters for a project ordered by SortOrder ascending.
    /// </summary>
    Task<IEnumerable<ProjectCounter>> GetByProjectIdAsync(Guid projectId);

    Task AddAsync(ProjectCounter counter);

    /// <summary>
    /// Hard deletes a counter and its history via ExecuteDeleteAsync.
    /// No SaveChanges needed — executes immediately.
    /// </summary>
    Task DeleteAsync(Guid counterId);

    /// <summary>
    /// Updates the counter's CurrentCount.
    /// When isPrimary is true, also syncs Project.CurrentCount so
    /// session tracking, export, and project cards stay accurate.
    /// </summary>
    Task UpdateCountAsync(Guid counterId, int newCount, bool isPrimary, Guid projectId);

    /// <summary>
    /// Updates the counter's name.
    /// No SaveChanges needed — executes immediately via ExecuteUpdateAsync.
    /// </summary>
    Task RenameAsync(Guid counterId, string newName);

    Task<int> SaveChangesAsync();
}
