using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Services;

/// <summary>
/// Coordinates semantic relationships between generic Workbench entries.
/// </summary>
public sealed class EntryLinkService
{
    private readonly IEntryLinkRepository _links;
    private readonly IEntryRepository _entries;
    private readonly IClock _clock;

    public EntryLinkService(IEntryLinkRepository links, IEntryRepository entries, IClock clock)
    {
        _links = links ?? throw new ArgumentNullException(nameof(links));
        _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task<IReadOnlyList<EntryLink>> ListForEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
        => _links.ListForEntryAsync(entryId, cancellationToken);

    public async Task<EntryLink> CreateAsync(
        Guid sourceEntryId,
        Guid targetEntryId,
        string relationType = "related_to",
        string? comment = null,
        string? createdBy = null,
        CancellationToken cancellationToken = default)
    {
        var source = await RequireEntryAsync(sourceEntryId, cancellationToken).ConfigureAwait(false);
        var target = await RequireEntryAsync(targetEntryId, cancellationToken).ConfigureAwait(false);
        if (source.ProjectId != target.ProjectId)
        {
            throw new InvalidOperationException("V1 entry links must connect entries within the same project.");
        }

        var link = new EntryLink(Guid.NewGuid(), sourceEntryId, targetEntryId, relationType, _clock.UtcNow, comment, createdBy);
        await _links.AddAsync(link, cancellationToken).ConfigureAwait(false);
        return link;
    }

    public async Task DeleteAsync(Guid linkId, CancellationToken cancellationToken = default)
    {
        var link = await _links.GetByIdAsync(linkId, cancellationToken).ConfigureAwait(false);
        if (link is null || link.IsDeleted)
        {
            return;
        }

        link.Delete();
        await _links.UpdateAsync(link, cancellationToken).ConfigureAwait(false);
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
