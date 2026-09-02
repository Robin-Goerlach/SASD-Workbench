using System.Security.Cryptography;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Models;
using SASD.Workbench.Infrastructure.Configuration;

namespace SASD.Workbench.Infrastructure.FileStorage;

/// <summary>
/// Copies attachment files into the Workbench data directory and computes integrity metadata.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly WorkbenchDataPaths _paths;

    public LocalFileStorageService(WorkbenchDataPaths paths)
        => _paths = paths ?? throw new ArgumentNullException(nameof(paths));

    public async Task<StoredFileInfo> StoreAttachmentAsync(
        Guid projectId,
        Guid entryId,
        Guid attachmentId,
        string sourceFilePath,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id must not be empty.", nameof(projectId));
        }

        if (entryId == Guid.Empty)
        {
            throw new ArgumentException("Entry id must not be empty.", nameof(entryId));
        }

        if (attachmentId == Guid.Empty)
        {
            throw new ArgumentException("Attachment id must not be empty.", nameof(attachmentId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);
        var sourceFullPath = Path.GetFullPath(sourceFilePath);
        if (!File.Exists(sourceFullPath))
        {
            throw new FileNotFoundException("Attachment source file does not exist.", sourceFullPath);
        }

        var originalFileName = Path.GetFileName(sourceFullPath);
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new InvalidOperationException("The source path does not contain a valid file name.");
        }

        var extension = Path.GetExtension(originalFileName);
        var safeName = SanitizeFileName(originalFileName);
        var storedFileName = $"{attachmentId:N}_{safeName}";
        var relativeDirectory = Path.Combine($"project-{projectId:D}", $"entry-{entryId:D}");
        var relativePath = Path.Combine(relativeDirectory, storedFileName);
        var destinationFullPath = ResolveUnderAttachmentsRoot(relativePath);
        var destinationDirectory = Path.GetDirectoryName(destinationFullPath)
            ?? throw new InvalidOperationException("Attachment destination directory could not be resolved.");
        Directory.CreateDirectory(destinationDirectory);

        if (File.Exists(destinationFullPath))
        {
            throw new IOException($"Attachment destination already exists: {destinationFullPath}");
        }

        try
        {
            await using (var source = new FileStream(sourceFullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var destination = new FileStream(destinationFullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
            }

            var fileInfo = new FileInfo(destinationFullPath);
            var hash = await ComputeSha256Async(destinationFullPath, cancellationToken).ConfigureAwait(false);

            return new StoredFileInfo(
                originalFileName,
                storedFileName,
                relativePath.Replace(Path.DirectorySeparatorChar, '/'),
                fileInfo.Length,
                hash,
                string.IsNullOrWhiteSpace(extension) ? null : extension.ToLowerInvariant(),
                GuessMimeType(extension));
        }
        catch
        {
            TryDelete(destinationFullPath);
            throw;
        }
    }

    public Task DeleteStoredFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        var path = ResolveUnderAttachmentsRoot(relativePath.Replace('/', Path.DirectorySeparatorChar));
        TryDelete(path);
        return Task.CompletedTask;
    }

    private string ResolveUnderAttachmentsRoot(string relativePath)
    {
        var root = Path.GetFullPath(_paths.AttachmentsDirectory);
        var candidate = Path.GetFullPath(Path.Combine(root, relativePath));
        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Attachment path escapes the controlled Workbench storage directory.");
        }

        return candidate;
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var characters = fileName.Select(character => invalid.Contains(character) ? '_' : character).ToArray();
        var result = new string(characters).Trim();
        return string.IsNullOrWhiteSpace(result) ? "attachment" : result;
    }

    private static string? GuessMimeType(string extension)
        => extension.ToLowerInvariant() switch
        {
            ".txt" => "text/plain",
            ".md" => "text/markdown",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".html" or ".htm" => "text/html",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            _ => null
        };

    private static void TryDelete(string path)
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
            // Best-effort cleanup; callers handle the original operation failure.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort cleanup; callers handle the original operation failure.
        }
    }
}
