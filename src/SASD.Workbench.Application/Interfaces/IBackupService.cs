using SASD.Workbench.Application.Models;

namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Defines full local Workbench backup and restore operations.
/// </summary>
public interface IBackupService
{
    Task<BackupResult> CreateBackupAsync(string destinationDirectory, CancellationToken cancellationToken = default);
    Task<RestoreResult> RestoreBackupAsync(string archivePath, CancellationToken cancellationToken = default);
}
