using System.Security.Cryptography;
using System.Text;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Models;
using SASD.Workbench.Application.Services;
using SASD.Workbench.Infrastructure.Backup;
using SASD.Workbench.Infrastructure.Configuration;
using SASD.Workbench.Infrastructure.Database;
using SASD.Workbench.Infrastructure.Export;
using SASD.Workbench.Infrastructure.FileStorage;
using SASD.Workbench.Infrastructure.Repositories;

namespace SASD.Workbench.SmokeTests;

internal static class Program
{
    private static async Task<int> Main()
    {
        var root = Path.Combine(Path.GetTempPath(), "SASD-Workbench-SmokeTests", Guid.NewGuid().ToString("N"));

        try
        {
            var paths = new WorkbenchDataPaths(root);
            paths.EnsureDirectories();
            Assert(Directory.Exists(paths.AttachmentsDirectory), "Attachments directory was not created.");

            var connections = new SqliteConnectionFactory(paths.DatabasePath);
            var migrator = new DatabaseMigrator(connections);
            await migrator.MigrateAsync();
            await migrator.MigrateAsync();

            var projectRepository = new SqliteProjectRepository(connections);
            var entryRepository = new SqliteEntryRepository(connections);
            var templateRepository = new SqliteTemplateRepository(connections);
            var tagRepository = new SqliteTagRepository(connections);
            var attachmentRepository = new SqliteAttachmentRepository(connections);
            var collectionRepository = new SqliteCollectionRepository(connections);
            var linkRepository = new SqliteEntryLinkRepository(connections);
            var activityRepository = new SqliteActivityLogRepository(connections);
            var fileStorage = new LocalFileStorageService(paths);
            var clock = new TestClock(new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc));

            var projectService = new ProjectService(projectRepository, clock);
            var entryService = new EntryService(projectRepository, entryRepository, clock);
            var templateService = new TemplateService(templateRepository, projectRepository, entryRepository, clock);
            var tagService = new TagService(tagRepository, entryRepository, clock);
            var attachmentService = new AttachmentService(attachmentRepository, entryRepository, projectRepository, fileStorage, clock);
            var collectionService = new CollectionService(collectionRepository, projectRepository, entryRepository, clock);
            var linkService = new EntryLinkService(linkRepository, entryRepository, clock);
            var activityService = new ActivityLogService(activityRepository, clock);
            var searchService = new SearchService(entryRepository);
            var exportService = new MarkdownProjectExportService(
                projectRepository,
                entryRepository,
                tagRepository,
                collectionRepository,
                attachmentRepository,
                paths,
                clock);
            var backupService = new LocalBackupService(connections, paths, clock);

            var project = await projectService.CreateAsync("Core smoke test", "Persistence round-trip", "general");
            Assert(project.Version == 1, "A new project must start at version 1.");

            var projects = await projectService.ListAsync();
            Assert(projects.Count == 1 && projects[0].Id == project.Id, "Project round-trip failed.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var updatedProject = await projectService.UpdateAsync(project.Id, "Core smoke test renamed", "Updated", "general");
            Assert(updatedProject.Version == 2, "Updating a project must advance its version once.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var entry = await entryService.CreateAsync(
                project.Id,
                "note",
                "First entry",
                "Initial summary",
                "# First entry\n\nInitial body.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var savedEntry = await entryService.UpdateAsync(
                entry.Id,
                "First entry updated",
                "Updated summary",
                "# First entry\n\nUpdated body with searchable phrase AlphaBeta.",
                "research_note",
                "in_work");
            Assert(savedEntry.Version == 2, "One logical entry save must advance the version exactly once.");

            var reloadedEntry = await entryService.GetByIdAsync(entry.Id)
                ?? throw new InvalidOperationException("The entry could not be reloaded.");
            Assert(reloadedEntry.Title == "First entry updated", "The updated entry title was not persisted.");
            Assert(reloadedEntry.EntryType == "research_note", "The updated entry type was not persisted.");
            Assert(reloadedEntry.Status == "in_work", "The updated entry status was not persisted.");
            Assert(reloadedEntry.Version == 2, "The persisted entry version is incorrect.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var template = await templateService.CreateAsync(
                "Research note",
                "research_note",
                "draft",
                "# Question\n\n## Sources\n\n## Findings\n",
                profileKey: "general",
                description: "Reusable research note template");
            var templateEntries = await templateService.ListAsync(profileKey: "general");
            Assert(templateEntries.Count == 1 && templateEntries[0].Id == template.Id, "Template round-trip failed.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var templatedEntry = await templateService.CreateEntryAsync(project.Id, template.Id, "Template-created entry");
            Assert(templatedEntry.EntryType == "research_note", "Template entry type was not copied.");
            Assert(templatedEntry.Status == "draft", "Template default status was not copied.");
            Assert(templatedEntry.ContentMarkdown.Contains("## Sources", StringComparison.Ordinal), "Template Markdown was not copied.");
            Assert(templatedEntry.Version == 1, "A template-created entry must begin at version 1.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var tag = await tagService.GetOrCreateAsync("Research");
            var sameTag = await tagService.GetOrCreateAsync(" research ");
            Assert(tag.Id == sameTag.Id, "Tag lookup must be case/whitespace normalized.");
            await tagService.AttachAsync(templatedEntry.Id, tag.Id);
            await tagService.AttachAsync(templatedEntry.Id, tag.Id);
            var tags = await tagService.ListByEntryAsync(templatedEntry.Id);
            Assert(tags.Count == 1 && tags[0].Id == tag.Id, "Tag assignment must be idempotent.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var rootCollection = await collectionService.CreateAsync(project.Id, "Research");
            var childCollection = await collectionService.CreateAsync(project.Id, "Sources", parentCollectionId: rootCollection.Id);
            await collectionService.AddEntryAsync(rootCollection.Id, templatedEntry.Id);
            await collectionService.AddEntryAsync(childCollection.Id, templatedEntry.Id);
            var entryCollections = await collectionService.ListByEntryAsync(templatedEntry.Id);
            Assert(entryCollections.Count == 2, "An entry must be able to belong to multiple collections.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var link = await linkService.CreateAsync(entry.Id, templatedEntry.Id, "related_to", "Smoke test relation");
            var links = await linkService.ListForEntryAsync(entry.Id);
            Assert(links.Count == 1 && links[0].Id == link.Id, "Entry relation round-trip failed.");

            clock.Advance(TimeSpan.FromMinutes(1));
            await activityService.RecordAsync(
                "smoke_test",
                "Verified V1 workflow relationships.",
                project.Id,
                templatedEntry.Id,
                newValue: "ok");
            var activity = await activityService.ListAsync(project.Id);
            Assert(activity.Count == 1 && activity[0].ActionType == "smoke_test", "Activity log round-trip failed.");

            var sourcePath = Path.Combine(root, "source-attachment.txt");
            const string sourceContent = "SASD Workbench controlled attachment smoke test.";
            await File.WriteAllTextAsync(sourcePath, sourceContent, Encoding.UTF8);
            var expectedHash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(sourcePath)));

            clock.Advance(TimeSpan.FromMinutes(1));
            var attachment = await attachmentService.AddAsync(templatedEntry.Id, sourcePath, "Smoke test attachment");
            Assert(attachment.Sha256Hash == expectedHash, "Attachment SHA-256 hash is incorrect.");
            var storedPath = Path.Combine(paths.AttachmentsDirectory, attachment.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert(File.Exists(storedPath), "Attachment was not copied into controlled storage.");
            Assert(await File.ReadAllTextAsync(storedPath) == sourceContent, "Stored attachment content differs from the source.");

            var textSearch = await searchService.SearchAsync(new EntrySearchQuery(Text: "AlphaBeta", ProjectId: project.Id));
            Assert(textSearch.Count == 1 && textSearch[0].Id == entry.Id, "Text search did not find content Markdown.");
            var tagSearch = await searchService.SearchAsync(new EntrySearchQuery(ProjectId: project.Id, TagId: tag.Id));
            Assert(tagSearch.Count == 1 && tagSearch[0].Id == templatedEntry.Id, "Tag filter failed.");
            var collectionSearch = await searchService.SearchAsync(new EntrySearchQuery(ProjectId: project.Id, CollectionId: childCollection.Id));
            Assert(collectionSearch.Count == 1 && collectionSearch[0].Id == templatedEntry.Id, "Collection filter failed.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var export = await exportService.ExportMarkdownAsync(project.Id, paths.ExportsDirectory);
            Assert(export.EntryCount == 2, "Markdown export entry count is incorrect.");
            Assert(export.AttachmentCount == 1, "Markdown export attachment count is incorrect.");
            Assert(File.Exists(Path.Combine(export.ExportDirectory, "README.md")), "Markdown export README is missing.");
            Assert(Directory.EnumerateFiles(Path.Combine(export.ExportDirectory, "entries"), "*.md").Count() == 2, "Markdown export entry files are missing.");
            Assert(Directory.EnumerateFiles(Path.Combine(export.ExportDirectory, "attachments"), "*", SearchOption.AllDirectories).Count() == 1, "Markdown export attachment copy is missing.");

            await VerifyMigrationCountAsync(connections, expectedCount: 3);

            clock.Advance(TimeSpan.FromMinutes(1));
            var backup = await backupService.CreateBackupAsync(paths.BackupsDirectory);
            Assert(File.Exists(backup.ArchivePath) && backup.ArchiveSize > 0, "Backup archive was not created.");

            // Deliberately damage/change live state after the backup, then prove restore returns to the backed-up state.
            clock.Advance(TimeSpan.FromMinutes(1));
            await entryService.DeleteAsync(entry.Id);
            await attachmentService.DeleteAsync(attachment.Id);
            File.Delete(storedPath);
            await projectService.CreateAsync("Created after backup");
            Assert((await projectService.ListAsync()).Count == 2, "Post-backup mutation setup failed.");
            Assert(!File.Exists(storedPath), "Physical attachment deletion setup failed.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var restore = await backupService.RestoreBackupAsync(backup.ArchivePath);
            Assert(!string.IsNullOrWhiteSpace(restore.SafetyBackupPath) && File.Exists(restore.SafetyBackupPath), "Restore did not create a safety backup of the replaced state.");

            projects = await projectService.ListAsync();
            Assert(projects.Count == 1 && projects[0].Id == project.Id, "Restore did not return the project set to its backed-up state.");
            var restoredEntry = await entryService.GetByIdAsync(entry.Id)
                ?? throw new InvalidOperationException("Restored entry is missing.");
            Assert(!restoredEntry.IsDeleted && restoredEntry.Title == "First entry updated", "Restored entry state is incorrect.");
            var restoredAttachments = await attachmentService.ListByEntryAsync(templatedEntry.Id);
            Assert(restoredAttachments.Count == 1 && restoredAttachments[0].Id == attachment.Id, "Restored attachment metadata is incorrect.");
            Assert(File.Exists(storedPath), "Restore did not restore the physical attachment.");
            Assert(await File.ReadAllTextAsync(storedPath) == sourceContent, "Restored attachment content is incorrect.");

            await VerifyMigrationCountAsync(connections, expectedCount: 3);

            Console.WriteLine("SASD Workbench V1 core smoke tests passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("SASD Workbench V1 core smoke tests FAILED.");
            Console.Error.WriteLine(ex);
            return 1;
        }
        finally
        {
            TryDeleteDirectory(root);
        }
    }

    private static async Task VerifyMigrationCountAsync(SqliteConnectionFactory connections, long expectedCount)
    {
        await using var connection = await connections.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM schema_migrations;";
        var actual = Convert.ToInt64(await command.ExecuteScalarAsync());
        Assert(actual == expectedCount, $"Expected {expectedCount} applied migration(s), found {actual}.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
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

    private sealed class TestClock : IClock
    {
        public TestClock(DateTime utcNow)
        {
            UtcNow = utcNow.Kind == DateTimeKind.Utc ? utcNow : utcNow.ToUniversalTime();
        }

        public DateTime UtcNow { get; private set; }
        public void Advance(TimeSpan amount) => UtcNow = UtcNow.Add(amount);
    }
}
