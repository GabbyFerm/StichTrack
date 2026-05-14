using StitchTrack.Domain.Entities;

namespace StitchTrack.Domain.Interfaces;

/// <summary>
/// Repository for ProjectFile — handles both pattern files and
/// inspiration photos attached to a project.
/// </summary>
public interface IProjectFileRepository
{
    Task AddAsync(ProjectFile file);

    /// <summary>Returns all files for a project ordered by upload date descending.</summary>
    Task<IEnumerable<ProjectFile>> GetByProjectIdAsync(Guid projectId);

    /// <summary>Returns files of a specific type for a project.</summary>
    Task<IEnumerable<ProjectFile>> GetByProjectIdAndTypeAsync(Guid projectId, ProjectFileType fileType);

    /// <summary>
    /// Hard deletes a file record by ID.
    /// The caller is responsible for deleting the physical file separately.
    /// </summary>
    Task DeleteAsync(Guid id);

    Task<int> SaveChangesAsync();
}
