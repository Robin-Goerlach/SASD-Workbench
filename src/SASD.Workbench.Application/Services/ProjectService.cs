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
}
