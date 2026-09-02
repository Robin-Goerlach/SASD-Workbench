using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Defines persistence operations for attachment metadata.
/// </summary>
public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Attachment>> ListByEntryAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task AddAsync(Attachment attachment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Attachment attachment, CancellationToken cancellationToken = default);
}
