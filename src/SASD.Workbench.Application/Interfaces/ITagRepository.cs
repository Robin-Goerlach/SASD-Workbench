using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Defines persistence operations required for reusable tags and entry-tag assignments.
/// </summary>
public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> ListByEntryAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
    Task AttachToEntryAsync(Guid entryId, Guid tagId, DateTime createdAtUtc, CancellationToken cancellationToken = default);
    Task DetachFromEntryAsync(Guid entryId, Guid tagId, CancellationToken cancellationToken = default);
}
