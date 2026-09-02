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
    /// Replaces editable content while preserving the entry identity and project relation.
    /// </summary>
    public void UpdateContent(string title, string? summary, string? contentMarkdown, DateTime updatedAtUtc)
    {
        Title = NormalizeRequired(title, nameof(title), 300);
        Summary = NormalizeOptional(summary);
        ContentMarkdown = contentMarkdown ?? string.Empty;
        UpdatedAtUtc = EnsureUtc(updatedAtUtc);
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
