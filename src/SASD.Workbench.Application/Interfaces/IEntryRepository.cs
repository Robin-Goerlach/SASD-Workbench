using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Defines persistence operations required by entry use cases.
/// </summary>
public interface IEntryRepository
{
    Task<Entry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Entry>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task AddAsync(Entry entry, CancellationToken cancellationToken = default);

    Task UpdateAsync(Entry entry, CancellationToken cancellationToken = default);
}
