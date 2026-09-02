namespace SASD.Workbench.Application.Models;

/// <summary>
/// Defines the intentionally simple V1 entry search and filter options.
/// </summary>
public sealed record EntrySearchQuery(
    string? Text = null,
    Guid? ProjectId = null,
    string? EntryType = null,
    string? Status = null,
    Guid? CollectionId = null,
    Guid? TagId = null,
    int Limit = 200);
