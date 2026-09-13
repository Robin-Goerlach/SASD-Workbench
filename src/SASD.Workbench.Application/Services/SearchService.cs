using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Models;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Services;

/// <summary>
/// Exposes the V1 text search and metadata filters without coupling the UI to SQLite.
/// </summary>
public sealed class SearchService
{
    private readonly IEntryRepository _entries;

    public SearchService(IEntryRepository entries)
        => _entries = entries ?? throw new ArgumentNullException(nameof(entries));

    public Task<IReadOnlyList<Entry>> SearchAsync(EntrySearchQuery query, CancellationToken cancellationToken = default)
        => _entries.SearchAsync(query, cancellationToken);
}
