namespace SASD.Workbench.Domain.Entities;

/// <summary>
/// Describes a file copied into controlled Workbench storage and associated with one entry.
/// </summary>
public sealed class Attachment
{
    private Attachment()
    {
    }

    public Attachment(
        Guid id,
        Guid entryId,
        string originalFileName,
        string storedFileName,
        string relativePath,
        long fileSize,
        string sha256Hash,
        DateTime createdAtUtc,
        string? mimeType = null,
        string? fileExtension = null,
        string? comment = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Attachment id must not be empty.", nameof(id));
        }

        if (entryId == Guid.Empty)
        {
            throw new ArgumentException("Entry id must not be empty.", nameof(entryId));
        }

        if (fileSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fileSize));
        }

        Id = id;
        EntryId = entryId;
        OriginalFileName = NormalizeRequired(originalFileName, nameof(originalFileName), 255);
        StoredFileName = NormalizeRequired(storedFileName, nameof(storedFileName), 400);
        RelativePath = NormalizeRequired(relativePath, nameof(relativePath), 1000);
        FileSize = fileSize;
        Sha256Hash = NormalizeHash(sha256Hash);
        MimeType = NormalizeOptional(mimeType);
        FileExtension = NormalizeOptional(fileExtension);
        Comment = NormalizeOptional(comment);
        CreatedAtUtc = EnsureUtc(createdAtUtc);
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid EntryId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string RelativePath { get; private set; } = string.Empty;
    public string? MimeType { get; private set; }
    public string? FileExtension { get; private set; }
    public long FileSize { get; private set; }
    public string Sha256Hash { get; private set; } = string.Empty;
    public string? Comment { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    public static Attachment Restore(
        Guid id,
        Guid entryId,
        string originalFileName,
        string storedFileName,
        string relativePath,
        string? mimeType,
        string? fileExtension,
        long fileSize,
        string sha256Hash,
        string? comment,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        bool isDeleted,
        DateTime? deletedAtUtc)
    {
        var attachment = new Attachment(
            id,
            entryId,
            originalFileName,
            storedFileName,
            relativePath,
            fileSize,
            sha256Hash,
            createdAtUtc,
            mimeType,
            fileExtension,
            comment)
        {
            UpdatedAtUtc = EnsureUtc(updatedAtUtc),
            IsDeleted = isDeleted,
            DeletedAtUtc = deletedAtUtc.HasValue ? EnsureUtc(deletedAtUtc.Value) : null
        };
        return attachment;
    }

    public void UpdateComment(string? comment, DateTime updatedAtUtc)
    {
        EnsureMutable();
        Comment = NormalizeOptional(comment);
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
            throw new InvalidOperationException("Deleted attachments cannot be modified.");
        }
    }

    private static string NormalizeHash(string value)
    {
        var normalized = NormalizeRequired(value, nameof(value), 64).ToUpperInvariant();
        if (normalized.Length != 64 || normalized.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException("SHA-256 hash must contain exactly 64 hexadecimal characters.", nameof(value));
        }

        return normalized;
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
