using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Models;
using SASD.Workbench.Application.Services;
using SASD.Workbench.Application.Templates;
using SASD.Workbench.Domain.Metadata;
using SASD.Workbench.Infrastructure.Configuration;
using SASD.Workbench.Infrastructure.Database;
using SASD.Workbench.Infrastructure.DependencyInjection;

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

            var clock = new TestClock(new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc));
            var services = new ServiceCollection();

            // Register the deterministic test clock before the shared Core. AddSasdWorkbenchCore uses
            // TryAdd for replaceable adapters, so hosts/tests can override them without copying the
            // production composition root.
            services.AddSingleton<IClock>(clock);
            services.AddSasdWorkbenchCore(paths);

            using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            var connections = serviceProvider.GetRequiredService<SqliteConnectionFactory>();
            var migrator = serviceProvider.GetRequiredService<DatabaseMigrator>();
            await migrator.MigrateAsync();
            await migrator.MigrateAsync();

            // Resolve every V1 use-case service through the same registration path used by hosts. This
            // turns the smoke test into a guard against composition-root drift as the product family grows.
            var projectService = serviceProvider.GetRequiredService<ProjectService>();
            var entryService = serviceProvider.GetRequiredService<EntryService>();
            var templateService = serviceProvider.GetRequiredService<TemplateService>();
            var tagService = serviceProvider.GetRequiredService<TagService>();
            var attachmentService = serviceProvider.GetRequiredService<AttachmentService>();
            var collectionService = serviceProvider.GetRequiredService<CollectionService>();
            var linkService = serviceProvider.GetRequiredService<EntryLinkService>();
            var activityService = serviceProvider.GetRequiredService<ActivityLogService>();
            var searchService = serviceProvider.GetRequiredService<SearchService>();
            var exportService = serviceProvider.GetRequiredService<IProjectExportService>();
            var backupService = serviceProvider.GetRequiredService<IBackupService>();

            Assert(CoreEntryTypes.IsBuiltIn(CoreEntryTypes.ResearchQuestion), "Research question must be a Core entry type.");
            Assert(CoreTemplateCatalog.All.Any(definition => definition.EntryType == CoreEntryTypes.ResearchQuestion), "Research question Core template is missing.");
            Assert(CoreTemplateCatalog.All.Any(definition => definition.EntryType == CoreEntryTypes.ResearchSource), "Research source Core template is missing.");
            Assert(CoreTemplateCatalog.All.Any(definition => definition.EntryType == CoreEntryTypes.Observation), "Observation Core template is missing.");
            Assert(CoreTemplateCatalog.All.Any(definition => definition.EntryType == CoreEntryTypes.Hypothesis), "Hypothesis Core template is missing.");
            Assert(CoreTemplateCatalog.All.Any(definition => definition.EntryType == CoreEntryTypes.Finding), "Finding Core template is missing.");
            Assert(CoreTemplateCatalog.All.Any(definition => definition.EntryType == CoreEntryTypes.Conclusion), "Conclusion Core template is missing.");
            Assert(CoreTemplateCatalog.All.Select(definition => definition.EntryType).Distinct(StringComparer.Ordinal).Count() == CoreTemplateCatalog.All.Count, "Core template entry types must be unique in V1.");

            Assert(EntryRelationTypes.IsBuiltIn(EntryRelationTypes.Supports), "Supports must be a built-in relation type.");
            Assert(EntryRelationTypes.Normalize(" Custom_Relation ") == "custom_relation", "Custom relation keys must be normalized.");
            AssertThrows<ArgumentException>(
                () => EntryRelationTypes.Normalize("invalid relation"),
                "Relation keys containing spaces must be rejected.");

            // Force the activity insert to fail once and prove that the primary project insert rolls
            // back with it. This protects the central invariant of the V1 lightweight history: for
            // SQLite-backed Core mutations, persisted state and its automatic activity record are atomic.
            await VerifyActivityFailureRollsBackMutationAsync(projectService, connections);

            var project = await projectService.CreateAsync("Core smoke test", "Persistence round-trip", "general");
            Assert(project.Version == 1, "A new project must start at version 1.");

            var projects = await projectService.ListAsync();
            Assert(projects.Count == 1 && projects[0].Id == project.Id, "Project round-trip failed.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var updatedProject = await projectService.UpdateAsync(
                project.Id,
                project.Version,
                "Core smoke test renamed",
                "Updated",
                "general");
            Assert(updatedProject.Version == 2, "Updating a project must advance its version once.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var entry = await entryService.CreateAsync(
                project.Id,
                CoreEntryTypes.Note,
                "First entry",
                "Initial summary",
                "# First entry\n\nInitial body.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var savedEntry = await entryService.UpdateAsync(
                entry.Id,
                entry.Version,
                "First entry updated",
                "Updated summary",
                "# First entry\n\nUpdated body with searchable phrase AlphaBeta.",
                CoreEntryTypes.ResearchNote,
                "in_work");
            Assert(savedEntry.Version == 2, "One logical entry save must advance the version exactly once.");

            var reloadedEntry = await entryService.GetByIdAsync(entry.Id)
                ?? throw new InvalidOperationException("The entry could not be reloaded.");
            Assert(reloadedEntry.Title == "First entry updated", "The updated entry title was not persisted.");
            Assert(reloadedEntry.EntryType == CoreEntryTypes.ResearchNote, "The updated entry type was not persisted.");
            Assert(reloadedEntry.Status == "in_work", "The updated entry status was not persisted.");
            Assert(reloadedEntry.Version == 2, "The persisted entry version is incorrect.");

            var researchNoteDefinition = CoreTemplateCatalog.All.Single(definition => definition.EntryType == CoreEntryTypes.ResearchNote);
            clock.Advance(TimeSpan.FromMinutes(1));
            var template = await templateService.CreateAsync(
                researchNoteDefinition.Name,
                researchNoteDefinition.EntryType,
                researchNoteDefinition.DefaultStatus,
                researchNoteDefinition.ContentMarkdown,
                profileKey: "general",
                description: researchNoteDefinition.Description);
            var templateEntries = await templateService.ListAsync(profileKey: "general");
            Assert(templateEntries.Count == 1 && templateEntries[0].Id == template.Id, "Template round-trip failed.");

            // General templates are deliberately valid in specialist profiles. The repository listing
            // must mirror the CreateEntryAsync compatibility rule so future specialist hosts do not hide
            // a common template that they are otherwise allowed to use.
            clock.Advance(TimeSpan.FromMinutes(1));
            var specialistProject = await projectService.CreateAsync("Specialist template consumer", profileKey: "biblical");
            var specialistTemplates = await templateService.ListAsync(specialistProject.Id, specialistProject.ProfileKey);
            Assert(specialistTemplates.Any(candidate => candidate.Id == template.Id), "A general template was hidden from a specialist profile.");
            await projectService.DeleteAsync(specialistProject.Id, specialistProject.Version);
            Assert((await projectService.ListAsync()).Count == 1, "Deleted specialist test project remained in the active project list.");

            // User-managed templates can be project-local and soft-deleted independently of entries.
            clock.Advance(TimeSpan.FromMinutes(1));
            var disposableTemplate = await templateService.CreateAsync(
                "Disposable project template",
                CoreEntryTypes.Note,
                "draft",
                "# Disposable\n",
                projectId: project.Id,
                profileKey: project.ProfileKey,
                description: "Smoke-test template deletion");
            await templateService.DeleteAsync(disposableTemplate.Id);
            var projectTemplates = await templateService.ListAsync(project.Id, project.ProfileKey);
            Assert(!projectTemplates.Any(candidate => candidate.Id == disposableTemplate.Id), "Deleted template remained visible in template listing.");

            clock.Advance(TimeSpan.FromMinutes(1));
            var templatedEntry = await templateService.CreateEntryAsync(project.Id, template.Id, "Template-created entry");
            Assert(templatedEntry.EntryType == CoreEntryTypes.ResearchNote, "Template entry type was not copied.");
            Assert(templatedEntry.Status == researchNoteDefinition.DefaultStatus, "Template default status was not copied.");
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
            var link = await linkService.CreateAsync(entry.Id, templatedEntry.Id, EntryRelationTypes.RelatedTo, "Smoke test relation");
            var customLink = await linkService.CreateAsync(entry.Id, templatedEntry.Id, " Custom_Relation ", "Extensible relation vocabulary");
            Assert(customLink.RelationType == "custom_relation", "Custom relation type was not normalized before persistence.");
            var links = await linkService.ListForEntryAsync(entry.Id);
            Assert(links.Count == 2 && links.Any(candidate => candidate.Id == link.Id), "Entry relation round-trip failed.");

            clock.Advance(TimeSpan.FromMinutes(1));
            await activityService.RecordAsync(
                "smoke_test",
                "Verified V1 workflow relationships.",
                project.Id,
                templatedEntry.Id,
                newValue: "ok");
            var activity = await activityService.ListAsync(project.Id);
            Assert(activity.Any(item => item.ActionType == CoreActivityTypes.ProjectCreated), "Automatic project creation activity is missing.");
            Assert(activity.Any(item => item.ActionType == CoreActivityTypes.ProjectUpdated), "Automatic project update activity is missing.");
            Assert(activity.Count(item => item.ActionType == CoreActivityTypes.EntryCreated) == 2, "Each persisted entry creation must create one activity record.");
            Assert(activity.Any(item => item.ActionType == CoreActivityTypes.EntryUpdated), "Automatic entry update activity is missing.");
            Assert(activity.Count(item => item.ActionType == CoreActivityTypes.TagAttached) == 1, "Idempotent repeated tag assignment must not create duplicate activity.");
            Assert(activity.Count(item => item.ActionType == CoreActivityTypes.CollectionEntryAdded) == 2, "Collection membership activities are incomplete.");
            Assert(activity.Count(item => item.ActionType == CoreActivityTypes.RelationCreated) == 2, "Relation creation activities are incomplete.");
            Assert(activity.Any(item => item.ActionType == CoreActivityTypes.TemplateDeleted), "Automatic template deletion activity is missing.");
            Assert(activity.Any(item => item.ActionType == "smoke_test"), "Explicit activity recording round-trip failed.");

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

            clock.Advance(TimeSpan.FromMinutes(1));
            attachment = await attachmentService.UpdateCommentAsync(attachment.Id, "Updated smoke-test comment");
            Assert(attachment.Comment == "Updated smoke-test comment", "Attachment comment update did not return the new value.");
            var commentRoundTrip = await attachmentService.ListByEntryAsync(templatedEntry.Id);
            Assert(commentRoundTrip.Count == 1 && commentRoundTrip[0].Comment == "Updated smoke-test comment", "Attachment comment update was not persisted.");

            activity = await activityService.ListAsync(project.Id);
            Assert(activity.Count(item => item.ActionType == CoreActivityTypes.AttachmentAdded) == 1, "Attachment metadata activity is missing.");
            Assert(activity.Count(item => item.ActionType == CoreActivityTypes.AttachmentUpdated) == 1, "Attachment comment update activity is missing.");

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
            await entryService.DeleteAsync(entry.Id, savedEntry.Version);
            await attachmentService.DeleteAsync(attachment.Id);
            File.Delete(storedPath);
            await projectService.CreateAsync("Created after backup");
            Assert((await projectService.ListAsync()).Count == 2, "Post-backup mutation setup failed.");
            Assert(!File.Exists(storedPath), "Physical attachment deletion setup failed.");

            activity = await activityService.ListAsync(project.Id);
            Assert(activity.Any(item => item.ActionType == CoreActivityTypes.EntryDeleted), "Post-backup entry deletion activity is missing.");
            Assert(activity.Any(item => item.ActionType == CoreActivityTypes.AttachmentDeleted), "Post-backup attachment deletion activity is missing.");

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
            Assert(restoredAttachments[0].Comment == "Updated smoke-test comment", "Restore lost the attachment comment update.");
            Assert(File.Exists(storedPath), "Restore did not restore the physical attachment.");
            Assert(await File.ReadAllTextAsync(storedPath) == sourceContent, "Restored attachment content is incorrect.");

            // Activity history is part of the backed-up database state. The post-backup delete records
            // must disappear after restore just like the mutations they described.
            activity = await activityService.ListAsync(project.Id);
            Assert(!activity.Any(item => item.ActionType == CoreActivityTypes.EntryDeleted), "Restore kept activity that occurred only after the backup.");
            Assert(!activity.Any(item => item.ActionType == CoreActivityTypes.AttachmentDeleted), "Restore kept attachment activity that occurred only after the backup.");
            Assert(activity.Any(item => item.ActionType == CoreActivityTypes.AttachmentAdded), "Restore lost pre-backup activity history.");
            Assert(activity.Any(item => item.ActionType == CoreActivityTypes.AttachmentUpdated), "Restore lost pre-backup attachment update history.");

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

    private static async Task VerifyActivityFailureRollsBackMutationAsync(
        ProjectService projectService,
        SqliteConnectionFactory connections)
    {
        const string triggerName = "smoke_force_activity_failure";
        await ExecuteSqlAsync(
            connections,
            $"""
            CREATE TRIGGER {triggerName}
            BEFORE INSERT ON activity_log
            BEGIN
                SELECT RAISE(ABORT, 'forced activity failure');
            END;
            """);

        var failedAsExpected = false;
        try
        {
            await projectService.CreateAsync("Must roll back");
        }
        catch (SqliteException)
        {
            failedAsExpected = true;
        }
        finally
        {
            await ExecuteSqlAsync(connections, $"DROP TRIGGER IF EXISTS {triggerName};");
        }

        Assert(failedAsExpected, "Forced activity failure did not fail the enclosing mutation.");
        Assert((await projectService.ListAsync()).Count == 0, "Primary mutation was not rolled back when activity recording failed.");
    }

    private static async Task ExecuteSqlAsync(SqliteConnectionFactory connections, string sql)
    {
        await using var connection = await connections.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
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

    private static void AssertThrows<TException>(Action action, string message)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
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
