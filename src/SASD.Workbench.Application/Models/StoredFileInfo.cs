namespace SASD.Workbench.Application.Models;

/// <summary>
/// Describes the result of copying a source file into controlled Workbench storage.
/// </summary>
public sealed record StoredFileInfo(
    string OriginalFileName,
    string StoredFileName,
    string RelativePath,
    long FileSize,
    string Sha256Hash,
    string? FileExtension,
    string? MimeType);
