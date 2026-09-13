using SASD.Workbench.Domain.Metadata;

namespace SASD.Workbench.Domain.Entities;

/// <summary>
/// Represents a directed semantic relationship between two Workbench entries.
/// </summary>
public sealed class EntryLink
{
    private EntryLink()
    {
    }

    public EntryLink(
        Guid id,
        Guid sourceEntryId,
        Guid targetEntryId,
        string relationType,
        DateTime createdAtUtc,
        string? comment = null,
        string? createdBy = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entry link id must not be empty.", nameof(id));
        }

        if (sourceEntryId == Guid.Empty || targetEntryId == Guid.Empty)
        {
            throw new ArgumentException("Source and target entry ids must not be empty.");
        }

        if (sourceEntryId == targetEntryId)
        {
            throw new ArgumentException("An entry cannot link to itself.");
        }

        Id = id;
        SourceEntryId = sourceEntryId;
        TargetEntryId = targetEntryId;
        RelationType = EntryRelationTypes.Normalize(relationType);
        Comment = NormalizeOptional(comment);
        CreatedBy = NormalizeOptional(createdBy);
        CreatedAtUtc = EnsureUtc(createdAtUtc);
    }

    public Guid Id { get; private set; }
    public Guid SourceEntryId { get; private set; }
    public Guid TargetEntryId { get; private set; }
    public string RelationType { get; private set; } = EntryRelationTypes.RelatedTo;
    public string? Comment { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public static EntryLink Restore(
        Guid id,
        Guid sourceEntryId,
        Guid targetEntryId,
        string relationType,
        string? comment,
        DateTime createdAtUtc,
        string? createdBy,
        bool isDeleted)
    {
        var link = new EntryLink(id, sourceEntryId, targetEntryId, relationType, createdAtUtc, comment, createdBy)
        {
            IsDeleted = isDeleted
        };
        return link;
    }

    public void Delete() => IsDeleted = true;

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime EnsureUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
}
