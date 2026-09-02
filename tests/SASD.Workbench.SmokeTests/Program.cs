using System.Security.Cryptography;
using System.Text;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Services;
using SASD.Workbench.Infrastructure.Configuration;
using SASD.Workbench.Infrastructure.Database;
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

            // Running migrations twice must be harmless and must not duplicate schema records.
            await migrator.MigrateAsync();
            await migrator.MigrateAsync();

            var projectRepository = new SqliteProjectRepository(connections);
            var entryRepository = new SqliteEntryRepository(connections);
            var templateRepository = new SqliteTemplateRepository(connections);
            var tagRepository = new SqliteTagRepository(connections);
            var attachmentRepository = new SqliteAttachmentRepository(connections);
            var fileStorage = new LocalFileStorageService(paths);
            var clock = new TestClock(new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc));

            var projectService = new ProjectService(projectRepository, clock);
            var entryService = new EntryService(projectRepository, entryRepository, clock);
            var templateService = new TemplateService(templateRepository, projectRepository, entryRepository, clock);
            var tagService = new TagService(tagRepository, entryRepository, clock);
            var attachmentService = new AttachmentService(attachmentRepository, entryRepository, projectRepository, fileStorage, clock);

            var project = await projectService.CreateAsync("Core smoke test", "Persistence round-trip", "general");
            Assert(project.Version == 1, "A new project must start at version 1.");

            var projects = await projectService.ListAsync();
            Assert(projects.Count == 1, "Exactly one active project was expected.");
            Assert(projects[0].Id == project.Id, "The persisted project id changed during round-trip.");

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
            Assert(entry.Version == 1, "A new entry must start at version 1.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var savedEntry = await entryService.UpdateAsync(
                entry.Id,
                "First entry updated",
                "Updated summary",
                "# First entry\n\nUpdated body.",
                "research_note",
                "in_work");
            Assert(savedEntry.Version == 2, "One logical entry save must advance the version exactly once.");

            var reloadedEntry = await entryService.GetByIdAsync(entry.Id)
                ?? throw new InvalidOperationException("The entry could not be reloaded.");
            Assert(reloadedEntry.Title == "First entry updated", "The updated entry title was not persisted.");
            Assert(reloadedEntry.EntryType == "research_note", "The updated entry type was not persisted.");
            Assert(reloadedEntry.Status == "in_work", "The updated entry status was not persisted.");
            Assert(reloadedEntry.ContentMarkdown.Contains("Updated body", StringComparison.Ordinal), "Markdown content was not persisted.");
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

            var sourcePath = Path.Combine(root, "source-attachment.txt");
            const string sourceContent = "SASD Workbench controlled attachment smoke test.";
            await File.WriteAllTextAsync(sourcePath, sourceContent, Encoding.UTF8);
            var expectedHash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(sourcePath)));

            clock.Advance(TimeSpan.FromMinutes(1));
            var attachment = await attachmentService.AddAsync(templatedEntry.Id, sourcePath, "Smoke test attachment");
            Assert(attachment.Sha256Hash == expectedHash, "Attachment SHA-256 hash is incorrect.");
            Assert(attachment.FileSize > 0, "Attachment file size was not recorded.");
            var storedPath = Path.Combine(paths.AttachmentsDirectory, attachment.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert(File.Exists(storedPath), "Attachment was not copied into controlled storage.");
            Assert(await File.ReadAllTextAsync(storedPath) == sourceContent, "Stored attachment content differs from the source.");

            var attachments = await attachmentService.ListByEntryAsync(templatedEntry.Id);
            Assert(attachments.Count == 1 && attachments[0].Id == attachment.Id, "Attachment metadata round-trip failed.");
            clock.Advance(TimeSpan.FromMinutes(1));
            await attachmentService.DeleteAsync(attachment.Id);
            attachments = await attachmentService.ListByEntryAsync(templatedEntry.Id);
            Assert(attachments.Count == 0, "Soft-deleted attachments must not appear in normal lists.");
            Assert(File.Exists(storedPath), "Soft-delete must retain the physical attachment until explicit cleanup.");

            var entries = await entryService.ListByProjectAsync(project.Id);
            Assert(entries.Count == 2, "Two active entries were expected before deleting the original entry.");

            clock.Advance(TimeSpan.FromMinutes(1));
            await entryService.DeleteAsync(entry.Id);
            entries = await entryService.ListByProjectAsync(project.Id);
            Assert(entries.Count == 1 && entries[0].Id == templatedEntry.Id, "Soft-deleted entries must not appear in the normal project list.");

            var deletedEntry = await entryRepository.GetByIdAsync(entry.Id)
                ?? throw new InvalidOperationException("The soft-deleted entry could not be reloaded.");
            Assert(deletedEntry.IsDeleted, "Soft-delete state was not persisted.");

            await VerifyMigrationCountAsync(connections, expectedCount: 2);

            Console.WriteLine("SASD Workbench V0.5 smoke tests passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("SASD Workbench V0.5 smoke tests FAILED.");
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
            // Cleanup must not hide the actual test result on platforms where SQLite releases files lazily.
        }
        catch (UnauthorizedAccessException)
        {
            // Same rationale as above; the temporary directory can be removed by the runner later.
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
