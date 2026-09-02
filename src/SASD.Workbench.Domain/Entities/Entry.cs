namespace SASD.Workbench.Domain.Entities;

/// <summary>
/// Represents the generic content unit of the Workbench core.
/// </summary>
public sealed class Entry
{
    private Entry()
    {
    }

    /// <summary>
    /// Creates a new entry without introducing profile-specific domain rules.
    /// </summary>
    public Entry(Guid id, Guid projectId, string entryType, string title, string? summary, string? contentMarkdown, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entry id must not be empty.", nameof(id));
        }

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id must not be empty.", nameof(projectId));
        }

        Id = id;
        ProjectId = projectId;
        EntryType = NormalizeRequired(entryType, nameof(entryType), 100);
        Title = NormalizeRequired(title, nameof(title), 300);
        Summary = NormalizeOptional(summary);
        ContentMarkdown = contentMarkdown ?? string.Empty;
        Status = "draft";
        CreatedAtUtc = EnsureUtc(createdAtUtc);
        UpdatedAtUtc = CreatedAtUtc;
        Version = 1;
    }

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string EntryType { get; private set; } = string.Empty;
    public string Status { get; private set; } = "draft";
    public string Title { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public string ContentMarkdown { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public long Version { get; private set; }
    public bool IsArchived { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    /// <summary>
    /// Recreates an entry from trusted persistence data.
    /// </summary>
    public static Entry Restore(
        Guid id,
        Guid projectId,
        string entryType,
        string status,
        string title,
        string? summary,
        string contentMarkdown,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        long version,
        bool isArchived,
        bool isDeleted,
        DateTime? deletedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entry id must not be empty.", nameof(id));
        }

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id must not be empty.", nameof(projectId));
        }

        if (version < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Version must be at least 1.");
        }

        return new Entry
        {
            Id = id,
            ProjectId = projectId,
            EntryType = NormalizeRequired(entryType, nameof(entryType), 100),
            Status = NormalizeRequired(status, nameof(status), 50),
            Title = NormalizeRequired(title, nameof(title), 300),
            Summary = NormalizeOptional(summary),
            ContentMarkdown = contentMarkdown ?? string.Empty,
            CreatedAtUtc = EnsureUtc(createdAtUtc),
            UpdatedAtUtc = EnsureUtc(updatedAtUtc),
            Version = version,
            IsArchived = isArchived,
            IsDeleted = isDeleted,
            DeletedAtUtc = deletedAtUtc.HasValue ? EnsureUtc(deletedAtUtc.Value) : null
        };
    }

    /// <summary>
    /// Updates all ordinary editable fields as one logical save operation.
    /// </summary>
    public void Update(
        string title,
        string? summary,
        string? contentMarkdown,
        string entryType,
        string status,
        DateTime updatedAtUtc)
    {
        EnsureMutable();
        Title = NormalizeRequired(title, nameof(title), 300);
        Summary = NormalizeOptional(summary);
        ContentMarkdown = contentMarkdown ?? string.Empty;
        EntryType = NormalizeRequired(entryType, nameof(entryType), 100);
        Status = NormalizeRequired(status, nameof(status), 50);
        Touch(updatedAtUtc);
    }

    /// <summary>
    /// Archives the entry without physically deleting it.
    /// </summary>
    public void Archive(DateTime updatedAtUtc)
    {
        EnsureMutable();
        IsArchived = true;
        Status = "archived";
        Touch(updatedAtUtc);
    }

    /// <summary>
    /// Restores an archived entry to draft state.
    /// </summary>
    public void Unarchive(DateTime updatedAtUtc)
    {
        EnsureMutable();
        IsArchived = false;
        Status = "draft";
        Touch(updatedAtUtc);
    }

    /// <summary>
    /// Soft-deletes the entry.
    /// </summary>
    public void Delete(DateTime deletedAtUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        var timestamp = EnsureUtc(deletedAtUtc);
        IsDeleted = true;
        DeletedAtUtc = timestamp;
        Status = "deleted";
        Touch(timestamp);
    }

    private void Touch(DateTime timestampUtc)
    {
        UpdatedAtUtc = EnsureUtc(timestampUtc);
        Version++;
    }

    private void EnsureMutable()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Deleted entries cannot be modified.");
        }
    }

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
