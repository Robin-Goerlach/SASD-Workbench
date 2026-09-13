using System.Text;
using SASD.Workbench.Application.Exceptions;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Models;
using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Metadata;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Tests;

/// <summary>
/// Focused regressions for technical boundaries that are easy to miss in pure unit tests.
/// </summary>
public sealed class InfrastructureRegressionTests
{
    [Fact]
    public async Task Migrations_AreIdempotentAndExpectedVersionCountIsApplied()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var workbench = await TestWorkbench.CreateAsync(cancellationToken);
        var migrator = workbench.GetRequiredService<DatabaseMigrator>();
        var connections = workbench.GetRequiredService<SqliteConnectionFactory>();

        // The fixture already migrated once. A second pass must be a no-op rather than applying the
        // same embedded script twice or introducing duplicate schema_migrations rows.
        await migrator.MigrateAsync(cancellationToken);

        await using var connection = await connections.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM schema_migrations;";
        var migrationCount = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        Assert.Equal(3, migrationCount);
    }

    [Fact]
    public async Task Search_TreatsPercentAndUnderscoreAsLiteralUserText()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var workbench = await TestWorkbench.CreateAsync(cancellationToken);
        var projects = workbench.GetRequiredService<ProjectService>();
        var entries = workbench.GetRequiredService<EntryService>();
        var search = workbench.GetRequiredService<SearchService>();
        var project = await projects.CreateAsync("Search escaping", cancellationToken: cancellationToken);

        var percentEntry = await entries.CreateAsync(
            project.Id,
            CoreEntryTypes.Note,
            "Percent",
            contentMarkdown: "Measured 100% completion.",
            cancellationToken: cancellationToken);
        var underscoreEntry = await entries.CreateAsync(
            project.Id,
            CoreEntryTypes.Note,
            "Underscore",
            contentMarkdown: "Machine key custom_relation.",
            cancellationToken: cancellationToken);
        await entries.CreateAsync(
            project.Id,
            CoreEntryTypes.Note,
            "Control",
            contentMarkdown: "Plain text without wildcard characters.",
            cancellationToken: cancellationToken);

        var percentResults = await search.SearchAsync(
            new EntrySearchQuery(Text: "%", ProjectId: project.Id),
            cancellationToken);
        var underscoreResults = await search.SearchAsync(
            new EntrySearchQuery(Text: "_", ProjectId: project.Id),
            cancellationToken);

        Assert.Single(percentResults);
        Assert.Equal(percentEntry.Id, percentResults[0].Id);
        Assert.Single(underscoreResults);
        Assert.Equal(underscoreEntry.Id, underscoreResults[0].Id);
    }

    [Fact]
    public async Task EntryRepository_SecondWriterFromSameVersion_IsRejectedAndFirstWriteSurvives()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var workbench = await TestWorkbench.CreateAsync(cancellationToken);
        var projects = workbench.GetRequiredService<ProjectService>();
        var entries = workbench.GetRequiredService<EntryService>();
        var entryRepository = workbench.GetRequiredService<IEntryRepository>();
        var project = await projects.CreateAsync("Entry concurrency", cancellationToken: cancellationToken);
        var created = await entries.CreateAsync(
            project.Id,
            CoreEntryTypes.Note,
            "Original",
            contentMarkdown: "Initial body",
            cancellationToken: cancellationToken);

        // Two independent repository reads simulate two editors that both loaded version 1.
        // The first save wins. The second must not silently reinterpret its stale data as version 2.
        var firstWriter = await entryRepository.GetByIdAsync(created.Id, cancellationToken)
            ?? throw new InvalidOperationException("First entry copy could not be loaded.");
        var staleWriter = await entryRepository.GetByIdAsync(created.Id, cancellationToken)
            ?? throw new InvalidOperationException("Stale entry copy could not be loaded.");

        firstWriter.Update(
            "First writer",
            null,
            "First writer body",
            CoreEntryTypes.Note,
            "draft",
            workbench.Clock.UtcNow.AddMinutes(1));
        await entryRepository.UpdateAsync(firstWriter, cancellationToken);

        staleWriter.Update(
            "Stale writer",
            null,
            "Stale writer body",
            CoreEntryTypes.Note,
            "draft",
            workbench.Clock.UtcNow.AddMinutes(2));
        var exception = await Assert.ThrowsAsync<OptimisticConcurrencyException>(
            () => entryRepository.UpdateAsync(staleWriter, cancellationToken));

        Assert.Equal(nameof(Domain.Entities.Entry), exception.EntityType);
        Assert.Equal(created.Id, exception.EntityId);
        Assert.Equal(1, exception.ExpectedVersion);
        Assert.Null(exception.ActualVersion);

        var persisted = await entryRepository.GetByIdAsync(created.Id, cancellationToken)
            ?? throw new InvalidOperationException("Entry disappeared after concurrency conflict.");
        Assert.Equal("First writer", persisted.Title);
        Assert.Equal("First writer body", persisted.ContentMarkdown);
        Assert.Equal(2, persisted.Version);
    }

    [Fact]
    public async Task ProjectRepository_SecondWriterFromSameVersion_IsRejectedAndFirstWriteSurvives()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var workbench = await TestWorkbench.CreateAsync(cancellationToken);
        var projects = workbench.GetRequiredService<ProjectService>();
        var projectRepository = workbench.GetRequiredService<IProjectRepository>();
        var created = await projects.CreateAsync("Project concurrency", cancellationToken: cancellationToken);

        var firstWriter = await projectRepository.GetByIdAsync(created.Id, cancellationToken)
            ?? throw new InvalidOperationException("First project copy could not be loaded.");
        var staleWriter = await projectRepository.GetByIdAsync(created.Id, cancellationToken)
            ?? throw new InvalidOperationException("Stale project copy could not be loaded.");

        firstWriter.Update("First project writer", "Saved first", "general", workbench.Clock.UtcNow.AddMinutes(1));
        await projectRepository.UpdateAsync(firstWriter, cancellationToken);

        staleWriter.Update("Stale project writer", "Must not win", "general", workbench.Clock.UtcNow.AddMinutes(2));
        var exception = await Assert.ThrowsAsync<OptimisticConcurrencyException>(
            () => projectRepository.UpdateAsync(staleWriter, cancellationToken));

        Assert.Equal(nameof(Domain.Entities.Project), exception.EntityType);
        Assert.Equal(created.Id, exception.EntityId);
        Assert.Equal(1, exception.ExpectedVersion);
        Assert.Null(exception.ActualVersion);

        var persisted = await projectRepository.GetByIdAsync(created.Id, cancellationToken)
            ?? throw new InvalidOperationException("Project disappeared after concurrency conflict.");
        Assert.Equal("First project writer", persisted.Name);
        Assert.Equal("Saved first", persisted.Description);
        Assert.Equal(2, persisted.Version);
    }

    [Fact]
    public async Task AttachmentActivityFailure_RollsBackMetadataAndDeletesCopiedFile()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var workbench = await TestWorkbench.CreateAsync(cancellationToken);
        var projects = workbench.GetRequiredService<ProjectService>();
        var entries = workbench.GetRequiredService<EntryService>();
        var attachments = workbench.GetRequiredService<AttachmentService>();
        var connections = workbench.GetRequiredService<SqliteConnectionFactory>();

        var project = await projects.CreateAsync("Attachment compensation", cancellationToken: cancellationToken);
        var entry = await entries.CreateAsync(
            project.Id,
            CoreEntryTypes.Note,
            "Entry",
            cancellationToken: cancellationToken);
        var sourcePath = Path.Combine(workbench.RootDirectory, "source.txt");
        await File.WriteAllTextAsync(
            sourcePath,
            "Attachment failure-path payload",
            Encoding.UTF8,
            cancellationToken);

        // Fail after attachment metadata was inserted but before its transaction can commit. This is
        // intentionally stronger than failing the metadata INSERT itself: it proves that the SQLite
        // transaction rolls metadata back and AttachmentService compensates the already-copied file.
        await using (var connection = await connections.OpenConnectionAsync(cancellationToken))
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"""
                CREATE TRIGGER test_fail_attachment_activity
                BEFORE INSERT ON activity_log
                WHEN NEW.action_type = '{CoreActivityTypes.AttachmentAdded}'
                BEGIN
                    SELECT RAISE(FAIL, 'forced attachment activity failure');
                END;
                """;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await Assert.ThrowsAnyAsync<Exception>(
            () => attachments.AddAsync(entry.Id, sourcePath, "Should roll back", cancellationToken));

        var persisted = await attachments.ListByEntryAsync(entry.Id, cancellationToken);
        Assert.Empty(persisted);
        Assert.Empty(Directory.EnumerateFiles(workbench.Paths.AttachmentsDirectory, "*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task AutomaticActivityFailure_RollsBackPrimaryProjectMutation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var workbench = await TestWorkbench.CreateAsync(cancellationToken);
        var projects = workbench.GetRequiredService<ProjectService>();
        var connections = workbench.GetRequiredService<SqliteConnectionFactory>();

        await using (var connection = await connections.OpenConnectionAsync(cancellationToken))
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"""
                CREATE TRIGGER test_fail_project_activity
                BEFORE INSERT ON activity_log
                WHEN NEW.action_type = '{CoreActivityTypes.ProjectCreated}'
                BEGIN
                    SELECT RAISE(FAIL, 'forced project activity failure');
                END;
                """;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await Assert.ThrowsAnyAsync<Exception>(
            () => projects.CreateAsync("Must not survive", cancellationToken: cancellationToken));
        Assert.Empty(await projects.ListAsync(cancellationToken));
    }
}
