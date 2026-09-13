namespace SASD.Workbench.Domain.Metadata;

/// <summary>
/// Defines stable machine-readable action keys used by the lightweight Workbench activity history.
/// </summary>
/// <remarks>
/// These values describe persisted Core mutations; they are intentionally not a regulatory audit
/// vocabulary. Keeping the keys centralized prevents different desktop hosts and future profiles from
/// inventing incompatible names for the same Core operation. Profiles may still add their own action
/// keys when the action is genuinely profile-specific.
/// </remarks>
public static class CoreActivityTypes
{
    public const string ProjectCreated = "project_created";
    public const string ProjectUpdated = "project_updated";
    public const string ProjectArchived = "project_archived";
    public const string ProjectDeleted = "project_deleted";

    public const string EntryCreated = "entry_created";
    public const string EntryUpdated = "entry_updated";
    public const string EntryArchived = "entry_archived";
    public const string EntryDeleted = "entry_deleted";

    public const string TemplateCreated = "template_created";
    public const string TemplateUpdated = "template_updated";
    public const string TemplateDeleted = "template_deleted";

    public const string TagCreated = "tag_created";
    public const string TagUpdated = "tag_updated";
    public const string TagDeleted = "tag_deleted";
    public const string TagAttached = "tag_attached";
    public const string TagDetached = "tag_detached";

    public const string CollectionCreated = "collection_created";
    public const string CollectionUpdated = "collection_updated";
    public const string CollectionDeleted = "collection_deleted";
    public const string CollectionEntryAdded = "collection_entry_added";
    public const string CollectionEntryRemoved = "collection_entry_removed";

    public const string RelationCreated = "relation_created";
    public const string RelationDeleted = "relation_deleted";

    public const string AttachmentAdded = "attachment_added";
    public const string AttachmentUpdated = "attachment_updated";
    public const string AttachmentDeleted = "attachment_deleted";
}
