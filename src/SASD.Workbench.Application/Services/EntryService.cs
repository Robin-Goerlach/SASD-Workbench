using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Services;

/// <summary>
/// Coordinates generic entry use cases without depending on a concrete Workbench profile.
/// </summary>
public sealed class EntryService
{
    private readonly IProjectRepository _projects;
    private readonly IEntryRepository _entries;
    private readonly IClock _clock;

    public EntryService(IProjectRepository projects, IEntryRepository entries, IClock clock)
    {
        _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Creates a new generic entry after verifying that its project exists.
    /// </summary>
    public async Task<Entry> CreateAsync(
        Guid projectId,
        string entryType,
        string title,
        string? summary = null,
        string? contentMarkdown = null,
        CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project is null || project.IsDeleted)
        {
            throw new InvalidOperationException($"Project '{projectId}' does not exist or is deleted.");
        }

        var entry = new Entry(Guid.NewGuid(), projectId, entryType, title, summary, contentMarkdown, _clock.UtcNow);
        await _entries.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        return entry;
    }
}
