namespace SASD.Workbench.Application.Models;

/// <summary>
/// Describes a completed Workbench backup archive.
/// </summary>
public sealed record BackupResult(string ArchivePath, DateTime CreatedAtUtc, long ArchiveSize);

/// <summary>
/// Describes a completed Workbench restore operation.
/// </summary>
public sealed record RestoreResult(string ArchivePath, DateTime RestoredAtUtc, string? SafetyBackupPath);
