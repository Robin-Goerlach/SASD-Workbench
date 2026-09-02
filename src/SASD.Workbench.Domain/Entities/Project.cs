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
        Rename(name);
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
    /// Changes the project name while keeping basic invariants inside the domain layer.
    /// </summary>
    public void Rename(string name)
    {
        Name = NormalizeRequired(name, nameof(name), 200);
    }

    /// <summary>
    /// Marks the project as changed and advances its optimistic version counter.
    /// </summary>
    public void Touch(DateTime timestampUtc)
    {
        UpdatedAtUtc = EnsureUtc(timestampUtc);
        Version++;
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
