using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Defines persistence operations for semantic relationships between entries.
/// </summary>
public interface IEntryLinkRepository
{
    Task<EntryLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EntryLink>> ListForEntryAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task AddAsync(EntryLink link, CancellationToken cancellationToken = default);
    Task UpdateAsync(EntryLink link, CancellationToken cancellationToken = default);
}
