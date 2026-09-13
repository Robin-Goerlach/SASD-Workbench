using SASD.Workbench.Application.Exceptions;
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

    /// <summary>
    /// Updates an entry only when the caller still owns the version it originally loaded.
    /// </summary>
    /// <remarks>
    /// Re-reading an entry and silently applying stale editor fields to the newest version would defeat
    /// optimistic concurrency: an older editor could overwrite a newer save. The expected version is
    /// therefore part of the use-case contract. The repository repeats the check in SQL to close the
    /// race between this read and the conditional UPDATE.
    /// </remarks>
    public async Task<Entry> UpdateAsync(
        Guid id,
        long expectedVersion,
        string title,
        string? summary,
        string? contentMarkdown,
        string? entryType = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var entry = await RequireEntryAsync(id, cancellationToken).ConfigureAwait(false);
        EnsureExpectedVersion(entry, expectedVersion);

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

    /// <summary>
    /// Archives an entry only if the caller's previously observed version is still current.
    /// </summary>
    public async Task ArchiveAsync(Guid id, long expectedVersion, CancellationToken cancellationToken = default)
    {
        var entry = await RequireEntryAsync(id, cancellationToken).ConfigureAwait(false);
        EnsureExpectedVersion(entry, expectedVersion);
        entry.Archive(_clock.UtcNow);
        await _entries.UpdateAsync(entry, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Soft-deletes an entry only if the caller's previously observed version is still current.
    /// </summary>
    public async Task DeleteAsync(Guid id, long expectedVersion, CancellationToken cancellationToken = default)
    {
        var entry = await RequireEntryAsync(id, cancellationToken).ConfigureAwait(false);
        EnsureExpectedVersion(entry, expectedVersion);
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

    private static void EnsureExpectedVersion(Entry entry, long expectedVersion)
    {
        if (expectedVersion < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedVersion), "Expected version must be at least 1.");
        }

        if (entry.Version != expectedVersion)
        {
            throw new OptimisticConcurrencyException(
                nameof(Entry),
                entry.Id,
                expectedVersion,
                entry.Version);
        }
    }
}
