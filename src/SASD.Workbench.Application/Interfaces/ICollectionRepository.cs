using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Defines persistence operations for hierarchical collections and entry membership.
/// </summary>
public interface ICollectionRepository
{
    Task<Collection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Collection>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Collection>> ListByEntryAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task AddAsync(Collection collection, CancellationToken cancellationToken = default);
    Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default);
    Task AddEntryAsync(Guid collectionId, Guid entryId, DateTime createdAtUtc, CancellationToken cancellationToken = default);
    Task RemoveEntryAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken = default);
}
