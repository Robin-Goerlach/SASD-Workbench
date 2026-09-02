using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Services;
using SASD.Workbench.Infrastructure.Configuration;
using SASD.Workbench.Infrastructure.Database;
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
            var clock = new TestClock(new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc));
            var projectService = new ProjectService(projectRepository, clock);
            var entryService = new EntryService(projectRepository, entryRepository, clock);

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

            var reloadedEntry = await entryService.GetByIdAsync(entry.Id);
            Assert(reloadedEntry is not null, "The entry could not be reloaded.");
            Assert(reloadedEntry.Title == "First entry updated", "The updated entry title was not persisted.");
            Assert(reloadedEntry.EntryType == "research_note", "The updated entry type was not persisted.");
            Assert(reloadedEntry.Status == "in_work", "The updated entry status was not persisted.");
            Assert(reloadedEntry.ContentMarkdown.Contains("Updated body", StringComparison.Ordinal), "Markdown content was not persisted.");
            Assert(reloadedEntry.Version == 2, "The persisted entry version is incorrect.");

            var entries = await entryService.ListByProjectAsync(project.Id);
            Assert(entries.Count == 1, "Exactly one active entry was expected.");

            clock.Advance(TimeSpan.FromMinutes(1));
            await entryService.DeleteAsync(entry.Id);
            entries = await entryService.ListByProjectAsync(project.Id);
            Assert(entries.Count == 0, "Soft-deleted entries must not appear in the normal project list.");

            var deletedEntry = await entryRepository.GetByIdAsync(entry.Id);
            Assert(deletedEntry is not null && deletedEntry.IsDeleted, "Soft-delete state was not persisted.");

            await VerifyMigrationCountAsync(connections, expectedCount: 1);

            Console.WriteLine("SASD Workbench V0.1 smoke tests passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("SASD Workbench V0.1 smoke tests FAILED.");
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
