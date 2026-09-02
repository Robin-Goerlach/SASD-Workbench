using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Services;

/// <summary>
/// Coordinates project-related application use cases.
/// </summary>
public sealed class ProjectService
{
    private readonly IProjectRepository _projects;
    private readonly IClock _clock;

    public ProjectService(IProjectRepository projects, IClock clock)
    {
        _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default)
        => _projects.ListAsync(cancellationToken);

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _projects.GetByIdAsync(id, cancellationToken);

    /// <summary>
    /// Creates a neutral Workbench project. A profile key is metadata only at this stage.
    /// </summary>
    public async Task<Project> CreateAsync(
        string name,
        string? description = null,
        string profileKey = "general",
        CancellationToken cancellationToken = default)
    {
        var project = new Project(Guid.NewGuid(), name, description, profileKey, _clock.UtcNow);
        await _projects.AddAsync(project, cancellationToken).ConfigureAwait(false);
        return project;
    }

    public async Task<Project> UpdateAsync(
        Guid id,
        string name,
        string? description,
        string profileKey,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireProjectAsync(id, cancellationToken).ConfigureAwait(false);
        project.Update(name, description, profileKey, _clock.UtcNow);
        await _projects.UpdateAsync(project, cancellationToken).ConfigureAwait(false);
        return project;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await RequireProjectAsync(id, cancellationToken).ConfigureAwait(false);
        project.Archive(_clock.UtcNow);
        await _projects.UpdateAsync(project, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await RequireProjectAsync(id, cancellationToken).ConfigureAwait(false);
        project.Delete(_clock.UtcNow);
        await _projects.UpdateAsync(project, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Project> RequireProjectAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await _projects.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (project is null || project.IsDeleted)
        {
            throw new InvalidOperationException($"Project '{id}' does not exist or is deleted.");
        }

        return project;
    }
}
