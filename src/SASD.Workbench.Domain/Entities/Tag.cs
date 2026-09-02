namespace SASD.Workbench.Domain.Entities;

/// <summary>
/// Represents a reusable, profile-neutral label that can classify Workbench entries.
/// </summary>
public sealed class Tag
{
    private Tag()
    {
    }

    public Tag(Guid id, string name, DateTime createdAtUtc, string? color = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Tag id must not be empty.", nameof(id));
        }

        Id = id;
        Name = NormalizeName(name);
        NormalizedName = NormalizeKey(Name);
        Color = NormalizeOptional(color);
        CreatedAtUtc = EnsureUtc(createdAtUtc);
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string? Color { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    public static Tag Restore(
        Guid id,
        string name,
        string normalizedName,
        string? color,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        bool isDeleted)
    {
        var tag = new Tag(id, name, createdAtUtc, color)
        {
            NormalizedName = string.IsNullOrWhiteSpace(normalizedName) ? NormalizeKey(name) : normalizedName,
            UpdatedAtUtc = EnsureUtc(updatedAtUtc),
            IsDeleted = isDeleted
        };
        return tag;
    }

    public void Update(string name, string? color, DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Deleted tags cannot be modified.");
        }

        Name = NormalizeName(name);
        NormalizedName = NormalizeKey(Name);
        Color = NormalizeOptional(color);
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

    public static string NormalizeKey(string name) => NormalizeName(name).ToUpperInvariant();

    private static string NormalizeName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        if (normalized.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Tag name must not exceed 100 characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime EnsureUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
}
