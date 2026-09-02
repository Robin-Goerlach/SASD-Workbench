using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Defines persistence operations for the lightweight chronological activity log.
/// </summary>
public interface IActivityLogRepository
{
    Task AddAsync(ActivityLogItem item, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityLogItem>> ListAsync(Guid? projectId = null, Guid? entryId = null, int limit = 200, CancellationToken cancellationToken = default);
}
