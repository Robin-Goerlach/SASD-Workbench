using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Services;

/// <summary>
/// Writes and reads the V1 lightweight activity log. This service does not claim regulatory audit properties.
/// </summary>
public sealed class ActivityLogService
{
    private readonly IActivityLogRepository _activity;
    private readonly IClock _clock;

    public ActivityLogService(IActivityLogRepository activity, IClock clock)
    {
        _activity = activity ?? throw new ArgumentNullException(nameof(activity));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<ActivityLogItem> RecordAsync(
        string actionType,
        string description,
        Guid? projectId = null,
        Guid? entryId = null,
        string? oldValue = null,
        string? newValue = null,
        string? createdBy = null,
        CancellationToken cancellationToken = default)
    {
        var item = new ActivityLogItem(
            Guid.NewGuid(),
            actionType,
            description,
            _clock.UtcNow,
            projectId,
            entryId,
            oldValue,
            newValue,
            createdBy);
        await _activity.AddAsync(item, cancellationToken).ConfigureAwait(false);
        return item;
    }

    public Task<IReadOnlyList<ActivityLogItem>> ListAsync(
        Guid? projectId = null,
        Guid? entryId = null,
        int limit = 200,
        CancellationToken cancellationToken = default)
        => _activity.ListAsync(projectId, entryId, limit, cancellationToken);
}
