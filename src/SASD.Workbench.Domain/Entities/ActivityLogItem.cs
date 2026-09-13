namespace SASD.Workbench.Domain.Entities;

/// <summary>
/// Records a lightweight chronological activity. V1 activity records are not a tamper-proof audit trail.
/// </summary>
public sealed class ActivityLogItem
{
    public ActivityLogItem(
        Guid id,
        string actionType,
        string description,
        DateTime createdAtUtc,
        Guid? projectId = null,
        Guid? entryId = null,
        string? oldValue = null,
        string? newValue = null,
        string? createdBy = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Activity id must not be empty.", nameof(id));
        }

        if (projectId == Guid.Empty || entryId == Guid.Empty)
        {
            throw new ArgumentException("Optional project and entry ids must be null or non-empty.");
        }

        Id = id;
        ProjectId = projectId;
        EntryId = entryId;
        ActionType = NormalizeRequired(actionType, nameof(actionType), 100);
        Description = NormalizeRequired(description, nameof(description), 1000);
        OldValue = NormalizeOptional(oldValue);
        NewValue = NormalizeOptional(newValue);
        CreatedBy = NormalizeOptional(createdBy);
        CreatedAtUtc = EnsureUtc(createdAtUtc);
    }

    public Guid Id { get; }
    public Guid? ProjectId { get; }
    public Guid? EntryId { get; }
    public string ActionType { get; }
    public string Description { get; }
    public string? OldValue { get; }
    public string? NewValue { get; }
    public DateTime CreatedAtUtc { get; }
    public string? CreatedBy { get; }

    private static string NormalizeRequired(string value, string parameterName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Value must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime EnsureUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
}
