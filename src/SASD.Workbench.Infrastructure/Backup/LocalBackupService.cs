using System.IO.Compression;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Models;
using SASD.Workbench.Infrastructure.Configuration;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Backup;

/// <summary>
/// Creates and restores self-contained local Workbench backup archives.
/// </summary>
public sealed class LocalBackupService : IBackupService
{
    private const int CurrentBackupFormat = 1;
    private readonly SqliteConnectionFactory _connections;
    private readonly WorkbenchDataPaths _paths;
    private readonly IClock _clock;

    public LocalBackupService(SqliteConnectionFactory connections, WorkbenchDataPaths paths, IClock clock)
    {
        _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<BackupResult> CreateBackupAsync(string destinationDirectory, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
        var destination = Path.GetFullPath(destinationDirectory);
        Directory.CreateDirectory(destination);

        var createdAt = _clock.UtcNow;
        var fileName = $"sasd-workbench-backup-{createdAt:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..8]}.zip";
        var finalArchive = Path.Combine(destination, fileName);
        var temporaryArchive = finalArchive + ".tmp";
        var workspace = Path.Combine(Path.GetTempPath(), "SASD-Workbench-Backup", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snapshotPath = Path.Combine(workspace, "workbench.db");
            await CreateDatabaseSnapshotAsync(snapshotPath, cancellationToken).ConfigureAwait(false);
            await ValidateDatabaseAsync(snapshotPath, cancellationToken).ConfigureAwait(false);

            var manifest = new BackupManifest(
                CurrentBackupFormat,
                createdAt,
                "workbench.db",
                "attachments/",
                "SASD Workbench full local backup");
            var manifestPath = Path.Combine(workspace, "manifest.json");
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }),
                cancellationToken).ConfigureAwait(false);

            if (File.Exists(temporaryArchive))
            {
                File.Delete(temporaryArchive);
            }

            using (var archive = ZipFile.Open(temporaryArchive, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(manifestPath, "manifest.json", CompressionLevel.Optimal);
                archive.CreateEntryFromFile(snapshotPath, "workbench.db", CompressionLevel.Optimal);
                AddDirectoryToArchive(archive, _paths.AttachmentsDirectory, "attachments", cancellationToken);
            }

            File.Move(temporaryArchive, finalArchive, overwrite: false);
            var size = new FileInfo(finalArchive).Length;
            return new BackupResult(finalArchive, createdAt, size);
        }
        finally
        {
            TryDeleteFile(temporaryArchive);
            TryDeleteDirectory(workspace);
        }
    }

    public async Task<RestoreResult> RestoreBackupAsync(string archivePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
        var archiveFullPath = Path.GetFullPath(archivePath);
        if (!File.Exists(archiveFullPath))
        {
            throw new FileNotFoundException("Backup archive does not exist.", archiveFullPath);
        }

        var staging = Path.Combine(_paths.RootDirectory, $".restore-staging-{Guid.NewGuid():N}");
        var oldAttachments = Path.Combine(_paths.RootDirectory, $".restore-old-attachments-{Guid.NewGuid():N}");
        var oldDatabase = _connections.DatabasePath + $".restore-old-{Guid.NewGuid():N}";
        Directory.CreateDirectory(staging);
        string? safetyBackupPath = null;
        var attachmentsSwapped = false;
        var databaseSwapped = false;

        try
        {
            await ExtractAndValidateArchiveAsync(archiveFullPath, staging, cancellationToken).ConfigureAwait(false);
            var stagedDatabase = Path.Combine(staging, "workbench.db");
            await ValidateDatabaseAsync(stagedDatabase, cancellationToken).ConfigureAwait(false);

            // Preserve the current complete state before replacing any live data.
            if (File.Exists(_connections.DatabasePath))
            {
                var safety = await CreateBackupAsync(_paths.BackupsDirectory, cancellationToken).ConfigureAwait(false);
                safetyBackupPath = safety.ArchivePath;
            }

            var stagedAttachments = Path.Combine(staging, "attachments");
            Directory.CreateDirectory(stagedAttachments);
            SqliteConnection.ClearAllPools();

            // Swap attachments first. If the database swap then fails, the catch block restores them.
            if (Directory.Exists(_paths.AttachmentsDirectory))
            {
                Directory.Move(_paths.AttachmentsDirectory, oldAttachments);
            }
            Directory.Move(stagedAttachments, _paths.AttachmentsDirectory);
            attachmentsSwapped = true;

            if (File.Exists(_connections.DatabasePath))
            {
                File.Move(_connections.DatabasePath, oldDatabase);
            }
            File.Move(stagedDatabase, _connections.DatabasePath);
            databaseSwapped = true;

            TryDeleteDirectory(oldAttachments);
            TryDeleteFile(oldDatabase);
            return new RestoreResult(archiveFullPath, _clock.UtcNow, safetyBackupPath);
        }
        catch
        {
            SqliteConnection.ClearAllPools();

            if (databaseSwapped)
            {
                TryDeleteFile(_connections.DatabasePath);
            }
            if (File.Exists(oldDatabase) && !File.Exists(_connections.DatabasePath))
            {
                File.Move(oldDatabase, _connections.DatabasePath);
            }

            if (attachmentsSwapped)
            {
                TryDeleteDirectory(_paths.AttachmentsDirectory);
            }
            if (Directory.Exists(oldAttachments) && !Directory.Exists(_paths.AttachmentsDirectory))
            {
                Directory.Move(oldAttachments, _paths.AttachmentsDirectory);
            }

            throw;
        }
        finally
        {
            TryDeleteDirectory(staging);
            if (databaseSwapped)
            {
                TryDeleteFile(oldDatabase);
            }
            if (attachmentsSwapped)
            {
                TryDeleteDirectory(oldAttachments);
            }
        }
    }

    private async Task CreateDatabaseSnapshotAsync(string snapshotPath, CancellationToken cancellationToken)
    {
        await using var source = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = snapshotPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        };
        await using var destination = new SqliteConnection(builder.ToString());
        await destination.OpenAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        source.BackupDatabase(destination);
    }

    private static async Task ValidateDatabaseAsync(string databasePath, CancellationToken cancellationToken)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly
        };
        await using var connection = new SqliteConnection(builder.ToString());
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using (var integrity = connection.CreateCommand())
        {
            integrity.CommandText = "PRAGMA integrity_check;";
            var value = Convert.ToString(await integrity.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
            if (!string.Equals(value, "ok", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"SQLite integrity check failed: {value ?? "no result"}.");
            }
        }

        await using (var schema = connection.CreateCommand())
        {
            schema.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'schema_migrations';";
            var count = Convert.ToInt64(await schema.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
            if (count != 1)
            {
                throw new InvalidDataException("Backup database does not contain the Workbench migration table.");
            }
        }
    }

    private static async Task ExtractAndValidateArchiveAsync(string archivePath, string staging, CancellationToken cancellationToken)
    {
        using var archive = ZipFile.OpenRead(archivePath);
        var manifestEntry = archive.GetEntry("manifest.json")
            ?? throw new InvalidDataException("Backup archive does not contain manifest.json.");
        var databaseEntry = archive.GetEntry("workbench.db")
            ?? throw new InvalidDataException("Backup archive does not contain workbench.db.");

        BackupManifest? manifest;
        await using (var stream = manifestEntry.Open())
        {
            manifest = await JsonSerializer.DeserializeAsync<BackupManifest>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        if (manifest is null || manifest.FormatVersion != CurrentBackupFormat)
        {
            throw new InvalidDataException("Backup manifest uses an unsupported format version.");
        }

        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalized = entry.FullName.Replace('\\', '/');
            if (normalized.EndsWith("/", StringComparison.Ordinal))
            {
                continue;
            }

            var allowed = normalized is "manifest.json" or "workbench.db" || normalized.StartsWith("attachments/", StringComparison.Ordinal);
            if (!allowed)
            {
                throw new InvalidDataException($"Unexpected backup entry '{entry.FullName}'.");
            }

            var target = ResolveExtractionPath(staging, normalized);
            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? staging);
            await using var input = entry.Open();
            await using var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
            await input.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
        }

        if (!File.Exists(Path.Combine(staging, databaseEntry.FullName)))
        {
            throw new InvalidDataException("Database extraction failed.");
        }
    }

    private static string ResolveExtractionPath(string root, string relativePath)
    {
        var rootFull = Path.GetFullPath(root);
        var rootPrefix = rootFull.EndsWith(Path.DirectorySeparatorChar) ? rootFull : rootFull + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(rootFull, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Backup entry attempts to escape the restore staging directory.");
        }
        return candidate;
    }

    private static void AddDirectoryToArchive(ZipArchive archive, string sourceDirectory, string archiveRoot, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relative = Path.GetRelativePath(sourceDirectory, file).Replace(Path.DirectorySeparatorChar, '/');
            archive.CreateEntryFromFile(file, $"{archiveRoot}/{relative}", CompressionLevel.Optimal);
        }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed record BackupManifest(
        int FormatVersion,
        DateTime CreatedAtUtc,
        string DatabaseFile,
        string AttachmentsDirectory,
        string Description);
}
