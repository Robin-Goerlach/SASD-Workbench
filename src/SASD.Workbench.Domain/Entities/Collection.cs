namespace SASD.Workbench.Domain.Entities;

/// <summary>
/// Represents a project-scoped collection. Entries may belong to multiple collections.
/// </summary>
public sealed class Collection
{
    private Collection()
    {
    }

    public Collection(
        Guid id,
        Guid projectId,
        string name,
        DateTime createdAtUtc,
        Guid? parentCollectionId = null,
        string? description = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Collection id must not be empty.", nameof(id));
        }

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id must not be empty.", nameof(projectId));
        }

        if (parentCollectionId == Guid.Empty)
        {
            throw new ArgumentException("Parent collection id must be null or non-empty.", nameof(parentCollectionId));
        }

        if (parentCollectionId == id)
        {
            throw new ArgumentException("A collection cannot be its own parent.", nameof(parentCollectionId));
        }

        Id = id;
        ProjectId = projectId;
        ParentCollectionId = parentCollectionId;
        Name = NormalizeRequired(name, nameof(name), 200);
        Description = NormalizeOptional(description);
        CreatedAtUtc = EnsureUtc(createdAtUtc);
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid? ParentCollectionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    public static Collection Restore(
        Guid id,
        Guid projectId,
        Guid? parentCollectionId,
        string name,
        string? description,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        int sortOrder,
        bool isDeleted,
        DateTime? deletedAtUtc)
    {
        var collection = new Collection(id, projectId, name, createdAtUtc, parentCollectionId, description)
        {
            UpdatedAtUtc = EnsureUtc(updatedAtUtc),
            SortOrder = sortOrder,
            IsDeleted = isDeleted,
            DeletedAtUtc = deletedAtUtc.HasValue ? EnsureUtc(deletedAtUtc.Value) : null
        };
        return collection;
    }

    public void Update(
        string name,
        string? description,
        Guid? parentCollectionId,
        int sortOrder,
        DateTime updatedAtUtc)
    {
        EnsureMutable();
        if (parentCollectionId == Guid.Empty)
        {
            throw new ArgumentException("Parent collection id must be null or non-empty.", nameof(parentCollectionId));
        }

        if (parentCollectionId == Id)
        {
            throw new ArgumentException("A collection cannot be its own parent.", nameof(parentCollectionId));
        }

        Name = NormalizeRequired(name, nameof(name), 200);
        Description = NormalizeOptional(description);
        ParentCollectionId = parentCollectionId;
        SortOrder = sortOrder;
        UpdatedAtUtc = EnsureUtc(updatedAtUtc);
    }

    public void Delete(DateTime deletedAtUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        var timestamp = EnsureUtc(deletedAtUtc);
        IsDeleted = true;
        DeletedAtUtc = timestamp;
        UpdatedAtUtc = timestamp;
    }

    private void EnsureMutable()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Deleted collections cannot be modified.");
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
