namespace SASD.Workbench.Domain.Entities;

/// <summary>
/// Represents a neutral Workbench project that can later be specialized by a profile.
/// </summary>
public sealed class Project
{
    private Project()
    {
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    public Project(Guid id, string name, string? description, string profileKey, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Project id must not be empty.", nameof(id));
        }

        Id = id;
        Name = NormalizeRequired(name, nameof(name), 200);
        Description = NormalizeOptional(description);
        ProfileKey = NormalizeRequired(profileKey, nameof(profileKey), 100);
        Status = "active";
        CreatedAtUtc = EnsureUtc(createdAtUtc);
        UpdatedAtUtc = CreatedAtUtc;
        Version = 1;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ProfileKey { get; private set; } = "general";
    public string Status { get; private set; } = "active";
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public long Version { get; private set; }
    public bool IsArchived { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    /// <summary>
    /// Recreates a project from trusted persistence data without pretending it is newly created.
    /// </summary>
    public static Project Restore(
        Guid id,
        string name,
        string? description,
        string profileKey,
        string status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        long version,
        bool isArchived,
        bool isDeleted,
        DateTime? deletedAtUtc)
    {
        if (version < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Version must be at least 1.");
        }

        var project = new Project
        {
            Id = id == Guid.Empty ? throw new ArgumentException("Project id must not be empty.", nameof(id)) : id,
            Name = NormalizeRequired(name, nameof(name), 200),
            Description = NormalizeOptional(description),
            ProfileKey = NormalizeRequired(profileKey, nameof(profileKey), 100),
            Status = NormalizeRequired(status, nameof(status), 50),
            CreatedAtUtc = EnsureUtc(createdAtUtc),
            UpdatedAtUtc = EnsureUtc(updatedAtUtc),
            Version = version,
            IsArchived = isArchived,
            IsDeleted = isDeleted,
            DeletedAtUtc = deletedAtUtc.HasValue ? EnsureUtc(deletedAtUtc.Value) : null
        };

        return project;
    }

    /// <summary>
    /// Changes editable project metadata and advances the version counter.
    /// </summary>
    public void Update(string name, string? description, string profileKey, DateTime updatedAtUtc)
    {
        EnsureMutable();
        Name = NormalizeRequired(name, nameof(name), 200);
        Description = NormalizeOptional(description);
        ProfileKey = NormalizeRequired(profileKey, nameof(profileKey), 100);
        Touch(updatedAtUtc);
    }

    /// <summary>
    /// Archives the project without deleting its data.
    /// </summary>
    public void Archive(DateTime updatedAtUtc)
    {
        EnsureMutable();
        IsArchived = true;
        Status = "archived";
        Touch(updatedAtUtc);
    }

    /// <summary>
    /// Restores an archived project to active use.
    /// </summary>
    public void Unarchive(DateTime updatedAtUtc)
    {
        EnsureMutable();
        IsArchived = false;
        Status = "active";
        Touch(updatedAtUtc);
    }

    /// <summary>
    /// Soft-deletes the project so data can be retained for recovery and auditing.
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

    /// <summary>
    /// Marks the project as changed and advances its optimistic version counter.
    /// </summary>
    private void Touch(DateTime timestampUtc)
    {
        UpdatedAtUtc = EnsureUtc(timestampUtc);
        Version++;
    }

    private void EnsureMutable()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Deleted projects cannot be modified.");
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
