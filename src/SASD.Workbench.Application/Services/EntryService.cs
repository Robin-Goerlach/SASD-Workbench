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

    public Task<Entry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _entries.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<Entry>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        => _entries.ListByProjectAsync(projectId, cancellationToken);

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

    public async Task<Entry> UpdateAsync(
        Guid id,
        string title,
        string? summary,
        string? contentMarkdown,
        string? entryType = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var entry = await RequireEntryAsync(id, cancellationToken).ConfigureAwait(false);
        entry.Update(
            title,
            summary,
            contentMarkdown,
            string.IsNullOrWhiteSpace(entryType) ? entry.EntryType : entryType,
            string.IsNullOrWhiteSpace(status) ? entry.Status : status,
            _clock.UtcNow);

        await _entries.UpdateAsync(entry, cancellationToken).ConfigureAwait(false);
        return entry;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await RequireEntryAsync(id, cancellationToken).ConfigureAwait(false);
        entry.Archive(_clock.UtcNow);
        await _entries.UpdateAsync(entry, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await RequireEntryAsync(id, cancellationToken).ConfigureAwait(false);
        entry.Delete(_clock.UtcNow);
        await _entries.UpdateAsync(entry, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Entry> RequireEntryAsync(Guid id, CancellationToken cancellationToken)
    {
        var entry = await _entries.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entry is null || entry.IsDeleted)
        {
            throw new InvalidOperationException($"Entry '{id}' does not exist or is deleted.");
        }

        return entry;
    }
}
