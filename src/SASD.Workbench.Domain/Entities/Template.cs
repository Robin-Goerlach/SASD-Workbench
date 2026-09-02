namespace SASD.Workbench.Domain.Entities;

/// <summary>
/// Defines reusable default content for creating generic Workbench entries.
/// </summary>
public sealed class Template
{
    private Template()
    {
    }

    public Template(
        Guid id,
        string name,
        string entryType,
        string defaultStatus,
        string? contentMarkdown,
        DateTime createdAtUtc,
        Guid? projectId = null,
        string profileKey = "general",
        string? description = null,
        bool isSystemTemplate = false)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Template id must not be empty.", nameof(id));
        }

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id must be null or a non-empty Guid.", nameof(projectId));
        }

        Id = id;
        ProjectId = projectId;
        Name = NormalizeRequired(name, nameof(name), 200);
        Description = NormalizeOptional(description);
        ProfileKey = NormalizeRequired(profileKey, nameof(profileKey), 100);
        EntryType = NormalizeRequired(entryType, nameof(entryType), 100);
        DefaultStatus = NormalizeRequired(defaultStatus, nameof(defaultStatus), 50);
        ContentMarkdown = contentMarkdown ?? string.Empty;
        IsSystemTemplate = isSystemTemplate;
        CreatedAtUtc = EnsureUtc(createdAtUtc);
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid? ProjectId { get; private set; }
    public string ProfileKey { get; private set; } = "general";
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string EntryType { get; private set; } = "note";
    public string DefaultStatus { get; private set; } = "draft";
    public string ContentMarkdown { get; private set; } = string.Empty;
    public bool IsSystemTemplate { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public int SortOrder { get; private set; }

    public static Template Restore(
        Guid id,
        Guid? projectId,
        string profileKey,
        string name,
        string? description,
        string entryType,
        string defaultStatus,
        string contentMarkdown,
        bool isSystemTemplate,
        bool isDeleted,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        int sortOrder)
    {
        var template = new Template(id, name, entryType, defaultStatus, contentMarkdown, createdAtUtc, projectId, profileKey, description, isSystemTemplate)
        {
            IsDeleted = isDeleted,
            UpdatedAtUtc = EnsureUtc(updatedAtUtc),
            SortOrder = sortOrder
        };
        return template;
    }

    /// <summary>
    /// Updates editable template metadata and content.
    /// </summary>
    public void Update(
        string name,
        string? description,
        string profileKey,
        string entryType,
        string defaultStatus,
        string? contentMarkdown,
        int sortOrder,
        DateTime updatedAtUtc)
    {
        EnsureMutable();
        Name = NormalizeRequired(name, nameof(name), 200);
        Description = NormalizeOptional(description);
        ProfileKey = NormalizeRequired(profileKey, nameof(profileKey), 100);
        EntryType = NormalizeRequired(entryType, nameof(entryType), 100);
        DefaultStatus = NormalizeRequired(defaultStatus, nameof(defaultStatus), 50);
        ContentMarkdown = contentMarkdown ?? string.Empty;
        SortOrder = sortOrder;
        UpdatedAtUtc = EnsureUtc(updatedAtUtc);
    }

    public void Delete(DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        UpdatedAtUtc = EnsureUtc(updatedAtUtc);
    }

    private void EnsureMutable()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Deleted templates cannot be modified.");
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
