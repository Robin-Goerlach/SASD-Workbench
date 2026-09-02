using SASD.Workbench.Application.Models;

namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Abstracts controlled attachment storage from application use cases.
/// </summary>
public interface IFileStorageService
{
    Task<StoredFileInfo> StoreAttachmentAsync(
        Guid projectId,
        Guid entryId,
        Guid attachmentId,
        string sourceFilePath,
        CancellationToken cancellationToken = default);

    Task DeleteStoredFileAsync(string relativePath, CancellationToken cancellationToken = default);
}
