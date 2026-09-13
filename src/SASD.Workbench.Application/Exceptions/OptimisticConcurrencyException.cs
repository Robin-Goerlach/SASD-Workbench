namespace SASD.Workbench.Application.Exceptions;

/// <summary>
/// Indicates that a caller attempted to mutate an entity version that is no longer current.
/// </summary>
/// <remarks>
/// This exception belongs to the Application boundary rather than to SQLite. A desktop host, CLI or
/// future profile host needs the same semantic signal regardless of which persistence adapter detects
/// the conflict. The optional actual version is available when the Application service could observe
/// the newer state before attempting persistence; repository-level races may only know the expected
/// version because the row changed between the read and the conditional UPDATE.
/// </remarks>
public sealed class OptimisticConcurrencyException : InvalidOperationException
{
    public OptimisticConcurrencyException(
        string entityType,
        Guid entityId,
        long expectedVersion,
        long? actualVersion = null)
        : base(CreateMessage(entityType, entityId, expectedVersion, actualVersion))
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("Entity id must not be empty.", nameof(entityId));
        }
        if (expectedVersion < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedVersion), "Expected version must be at least 1.");
        }
        if (actualVersion is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(actualVersion), "Actual version must be at least 1 when supplied.");
        }

        EntityType = entityType.Trim();
        EntityId = entityId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public string EntityType { get; }
    public Guid EntityId { get; }
    public long ExpectedVersion { get; }
    public long? ActualVersion { get; }

    private static string CreateMessage(
        string entityType,
        Guid entityId,
        long expectedVersion,
        long? actualVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("Entity id must not be empty.", nameof(entityId));
        }
        if (expectedVersion < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedVersion), "Expected version must be at least 1.");
        }

        var normalizedType = entityType.Trim();
        return actualVersion.HasValue
            ? $"{normalizedType} '{entityId}' changed after version {expectedVersion}. The current version is {actualVersion.Value}. Reload the current state before saving your changes."
            : $"{normalizedType} '{entityId}' changed or was removed after version {expectedVersion}. Reload the current state before saving your changes.";
    }
}
