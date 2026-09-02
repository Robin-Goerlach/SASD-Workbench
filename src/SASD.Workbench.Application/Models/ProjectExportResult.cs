namespace SASD.Workbench.Application.Models;

/// <summary>
/// Describes the output of a project export operation.
/// </summary>
public sealed record ProjectExportResult(
    string ExportDirectory,
    int EntryCount,
    int AttachmentCount,
    DateTime CreatedAtUtc);
