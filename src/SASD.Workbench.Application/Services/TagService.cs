using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Services;

/// <summary>
/// Coordinates creation and assignment of reusable Workbench tags.
/// </summary>
public sealed class TagService
{
    private readonly ITagRepository _tags;
    private readonly IEntryRepository _entries;
    private readonly IClock _clock;

    public TagService(ITagRepository tags, IEntryRepository entries, IClock clock)
    {
        _tags = tags ?? throw new ArgumentNullException(nameof(tags));
        _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task<IReadOnlyList<Tag>> ListAsync(CancellationToken cancellationToken = default)
        => _tags.ListAsync(cancellationToken);

    public Task<IReadOnlyList<Tag>> ListByEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
        => _tags.ListByEntryAsync(entryId, cancellationToken);

    /// <summary>
    /// Returns an existing case-insensitive tag or creates a new one.
    /// </summary>
    public async Task<Tag> GetOrCreateAsync(string name, string? color = null, CancellationToken cancellationToken = default)
    {
        var normalized = Tag.NormalizeKey(name);
        var existing = await _tags.GetByNormalizedNameAsync(normalized, cancellationToken).ConfigureAwait(false);
        if (existing is not null && !existing.IsDeleted)
        {
            return existing;
        }

        if (existing is not null && existing.IsDeleted)
        {
            throw new InvalidOperationException($"Tag '{name.Trim()}' exists in deleted state and must be restored or renamed before reuse.");
        }

        var tag = new Tag(Guid.NewGuid(), name, _clock.UtcNow, color);
        await _tags.AddAsync(tag, cancellationToken).ConfigureAwait(false);
        return tag;
    }

    public async Task AttachAsync(Guid entryId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var entry = await _entries.GetByIdAsync(entryId, cancellationToken).ConfigureAwait(false);
        if (entry is null || entry.IsDeleted)
        {
            throw new InvalidOperationException($"Entry '{entryId}' does not exist or is deleted.");
        }

        var tag = await _tags.GetByIdAsync(tagId, cancellationToken).ConfigureAwait(false);
        if (tag is null || tag.IsDeleted)
        {
            throw new InvalidOperationException($"Tag '{tagId}' does not exist or is deleted.");
        }

        await _tags.AttachToEntryAsync(entryId, tagId, _clock.UtcNow, cancellationToken).ConfigureAwait(false);
    }

    public Task DetachAsync(Guid entryId, Guid tagId, CancellationToken cancellationToken = default)
        => _tags.DetachFromEntryAsync(entryId, tagId, cancellationToken);
}
