using System.Globalization;
using System.Text;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Models;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Infrastructure.Configuration;

namespace SASD.Workbench.Infrastructure.Export;

/// <summary>
/// Exports a project into a human-readable Markdown directory with copied attachments.
/// </summary>
public sealed class MarkdownProjectExportService : IProjectExportService
{
    private readonly IProjectRepository _projects;
    private readonly IEntryRepository _entries;
    private readonly ITagRepository _tags;
    private readonly ICollectionRepository _collections;
    private readonly IAttachmentRepository _attachments;
    private readonly WorkbenchDataPaths _paths;
    private readonly IClock _clock;

    public MarkdownProjectExportService(
        IProjectRepository projects,
        IEntryRepository entries,
        ITagRepository tags,
        ICollectionRepository collections,
        IAttachmentRepository attachments,
        WorkbenchDataPaths paths,
        IClock clock)
    {
        _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _tags = tags ?? throw new ArgumentNullException(nameof(tags));
        _collections = collections ?? throw new ArgumentNullException(nameof(collections));
        _attachments = attachments ?? throw new ArgumentNullException(nameof(attachments));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<ProjectExportResult> ExportMarkdownAsync(
        Guid projectId,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
        var project = await _projects.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project is null || project.IsDeleted)
        {
            throw new InvalidOperationException($"Project '{projectId}' does not exist or is deleted.");
        }

        var createdAt = _clock.UtcNow;
        var root = CreateUniqueExportDirectory(destinationDirectory, project, createdAt);
        var entriesDirectory = Path.Combine(root, "entries");
        var attachmentsDirectory = Path.Combine(root, "attachments");
        Directory.CreateDirectory(entriesDirectory);
        Directory.CreateDirectory(attachmentsDirectory);

        var entries = await _entries.ListByProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        var attachmentCount = 0;

        await File.WriteAllTextAsync(
            Path.Combine(root, "README.md"),
            BuildProjectReadme(project, entries.Count, createdAt),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            cancellationToken).ConfigureAwait(false);

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tags = await _tags.ListByEntryAsync(entry.Id, cancellationToken).ConfigureAwait(false);
            var collections = await _collections.ListByEntryAsync(entry.Id, cancellationToken).ConfigureAwait(false);
            var attachments = await _attachments.ListByEntryAsync(entry.Id, cancellationToken).ConfigureAwait(false);
            attachmentCount += attachments.Count;

            var entryAttachmentDirectory = Path.Combine(attachmentsDirectory, entry.Id.ToString("D"));
            if (attachments.Count > 0)
            {
                Directory.CreateDirectory(entryAttachmentDirectory);
                foreach (var attachment in attachments)
                {
                    CopyAttachment(attachment, entryAttachmentDirectory);
                }
            }

            var markdown = BuildEntryMarkdown(entry, tags, collections, attachments);
            var fileName = BuildEntryFileName(entry);
            await File.WriteAllTextAsync(
                Path.Combine(entriesDirectory, fileName),
                markdown,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken).ConfigureAwait(false);
        }

        return new ProjectExportResult(root, entries.Count, attachmentCount, createdAt);
    }

    private void CopyAttachment(Attachment attachment, string targetDirectory)
    {
        var source = ResolveAttachmentPath(attachment.RelativePath);
        if (!File.Exists(source))
        {
            throw new FileNotFoundException($"Stored attachment '{attachment.OriginalFileName}' is missing.", source);
        }

        var target = Path.Combine(targetDirectory, attachment.StoredFileName);
        File.Copy(source, target, overwrite: false);
    }

    private string ResolveAttachmentPath(string relativePath)
    {
        var root = Path.GetFullPath(_paths.AttachmentsDirectory);
        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Attachment path escapes the controlled storage directory.");
        }
        return candidate;
    }

    private static string CreateUniqueExportDirectory(string destinationDirectory, Project project, DateTime createdAtUtc)
    {
        var destination = Path.GetFullPath(destinationDirectory);
        Directory.CreateDirectory(destination);
        var baseName = $"{SanitizeFileName(project.Name)}-{createdAtUtc:yyyyMMdd-HHmmss}";
        var candidate = Path.Combine(destination, baseName);
        if (Directory.Exists(candidate))
        {
            candidate = Path.Combine(destination, $"{baseName}-{Guid.NewGuid():N}"[..(baseName.Length + 9)]);
        }
        Directory.CreateDirectory(candidate);
        return candidate;
    }

    private static string BuildProjectReadme(Project project, int entryCount, DateTime createdAtUtc)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {project.Name}");
        builder.AppendLine();
        if (!string.IsNullOrWhiteSpace(project.Description))
        {
            builder.AppendLine(project.Description);
            builder.AppendLine();
        }
        builder.AppendLine("## Export metadata");
        builder.AppendLine();
        builder.AppendLine($"- Project ID: `{project.Id:D}`");
        builder.AppendLine($"- Profile: `{project.ProfileKey}`");
        builder.AppendLine($"- Status: `{project.Status}`");
        builder.AppendLine($"- Entries: {entryCount}");
        builder.AppendLine($"- Exported UTC: `{createdAtUtc.ToString("O", CultureInfo.InvariantCulture)}`");
        builder.AppendLine();
        builder.AppendLine("Entry documents are stored in `entries/`; copied files are stored in `attachments/`.");
        return builder.ToString();
    }

    private static string BuildEntryMarkdown(
        Entry entry,
        IReadOnlyList<Tag> tags,
        IReadOnlyList<Collection> collections,
        IReadOnlyList<Attachment> attachments)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {entry.Title}");
        builder.AppendLine();
        builder.AppendLine("## Metadata");
        builder.AppendLine();
        builder.AppendLine($"- Entry ID: `{entry.Id:D}`");
        builder.AppendLine($"- Type: `{entry.EntryType}`");
        builder.AppendLine($"- Status: `{entry.Status}`");
        builder.AppendLine($"- Created UTC: `{entry.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture)}`");
        builder.AppendLine($"- Updated UTC: `{entry.UpdatedAtUtc.ToString("O", CultureInfo.InvariantCulture)}`");
        builder.AppendLine($"- Version: {entry.Version}");
        if (tags.Count > 0)
        {
            builder.AppendLine($"- Tags: {string.Join(", ", tags.Select(tag => tag.Name))}");
        }
        if (collections.Count > 0)
        {
            builder.AppendLine($"- Collections: {string.Join(", ", collections.Select(collection => collection.Name))}");
        }
        if (!string.IsNullOrWhiteSpace(entry.Summary))
        {
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine(entry.Summary);
        }
        builder.AppendLine();
        builder.AppendLine("## Content");
        builder.AppendLine();
        builder.AppendLine(entry.ContentMarkdown);

        if (attachments.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("## Attachments");
            builder.AppendLine();
            foreach (var attachment in attachments)
            {
                var link = $"../attachments/{entry.Id:D}/{Uri.EscapeDataString(attachment.StoredFileName)}";
                builder.AppendLine($"- [{attachment.OriginalFileName}]({link}) — SHA-256 `{attachment.Sha256Hash}`");
            }
        }
        return builder.ToString();
    }

    private static string BuildEntryFileName(Entry entry)
        => $"{entry.CreatedAtUtc:yyyyMMdd-HHmmss}_{SanitizeFileName(entry.Title)}_{entry.Id.ToString("N")[..8]}.md";

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Trim().Select(character => invalid.Contains(character) ? '_' : character).ToArray();
        var safe = new string(chars).Trim().TrimEnd('.');
        if (safe.Length > 100)
        {
            safe = safe[..100].TrimEnd();
        }
        return string.IsNullOrWhiteSpace(safe) ? "item" : safe;
    }
}
