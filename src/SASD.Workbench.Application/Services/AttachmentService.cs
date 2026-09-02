using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Services;

/// <summary>
/// Coordinates attachment metadata with controlled file storage.
/// </summary>
public sealed class AttachmentService
{
    private readonly IAttachmentRepository _attachments;
    private readonly IEntryRepository _entries;
    private readonly IProjectRepository _projects;
    private readonly IFileStorageService _fileStorage;
    private readonly IClock _clock;

    public AttachmentService(
        IAttachmentRepository attachments,
        IEntryRepository entries,
        IProjectRepository projects,
        IFileStorageService fileStorage,
        IClock clock)
    {
        _attachments = attachments ?? throw new ArgumentNullException(nameof(attachments));
        _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task<IReadOnlyList<Attachment>> ListByEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
        => _attachments.ListByEntryAsync(entryId, cancellationToken);

    /// <summary>
    /// Copies a source file into controlled Workbench storage and persists its metadata.
    /// </summary>
    public async Task<Attachment> AddAsync(
        Guid entryId,
        string sourceFilePath,
        string? comment = null,
        CancellationToken cancellationToken = default)
    {
        var entry = await _entries.GetByIdAsync(entryId, cancellationToken).ConfigureAwait(false);
        if (entry is null || entry.IsDeleted)
        {
            throw new InvalidOperationException($"Entry '{entryId}' does not exist or is deleted.");
        }

        var project = await _projects.GetByIdAsync(entry.ProjectId, cancellationToken).ConfigureAwait(false);
        if (project is null || project.IsDeleted)
        {
            throw new InvalidOperationException($"Project '{entry.ProjectId}' does not exist or is deleted.");
        }

        var attachmentId = Guid.NewGuid();
        var stored = await _fileStorage.StoreAttachmentAsync(
            project.Id,
            entry.Id,
            attachmentId,
            sourceFilePath,
            cancellationToken).ConfigureAwait(false);

        try
        {
            var attachment = new Attachment(
                attachmentId,
                entry.Id,
                stored.OriginalFileName,
                stored.StoredFileName,
                stored.RelativePath,
                stored.FileSize,
                stored.Sha256Hash,
                _clock.UtcNow,
                stored.MimeType,
                stored.FileExtension,
                comment);
            await _attachments.AddAsync(attachment, cancellationToken).ConfigureAwait(false);
            return attachment;
        }
        catch
        {
            await _fileStorage.DeleteStoredFileAsync(stored.RelativePath, CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Soft-deletes attachment metadata. Physical file deletion is intentionally deferred to explicit cleanup.
    /// </summary>
    public async Task DeleteAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await _attachments.GetByIdAsync(attachmentId, cancellationToken).ConfigureAwait(false);
        if (attachment is null || attachment.IsDeleted)
        {
            return;
        }

        attachment.Delete(_clock.UtcNow);
        await _attachments.UpdateAsync(attachment, cancellationToken).ConfigureAwait(false);
    }
}
