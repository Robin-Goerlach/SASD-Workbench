using SASD.Workbench.Application.Exceptions;
using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Domain.Metadata;

namespace SASD.Workbench.Application.Tests;

/// <summary>
/// Verifies the caller-facing optimistic-concurrency contract independently from SQLite.
/// </summary>
/// <remarks>
/// The important rule is that a caller must save against the version it originally loaded. Services
/// re-read the current entity to validate existence, but they must never reinterpret stale editor data
/// as an update of that newer version. Repository-level conditional updates provide a second guard for
/// the smaller race that can still happen after this Application check.
/// </remarks>
public sealed class OptimisticConcurrencyServiceTests
{
    private static readonly DateTime Now = new(2026, 9, 13, 19, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task EntryUpdateAsync_CurrentVersion_UpdatesAndAdvancesExactlyOnce()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projects = new InMemoryProjectRepository();
        var entries = new InMemoryEntryRepository();
        var project = new Project(Guid.NewGuid(), "Project", null, "general", Now);
        var entry = new Entry(Guid.NewGuid(), project.Id, CoreEntryTypes.Note, "Original", null, "Old body", Now);
        projects.Seed(project);
        entries.Seed(entry);
        var service = new EntryService(projects, entries, new TestClock(Now.AddMinutes(1)));

        var saved = await service.UpdateAsync(
            entry.Id,
            expectedVersion: 1,
            title: "Saved",
            summary: "Current caller",
            contentMarkdown: "New body",
            entryType: CoreEntryTypes.ResearchNote,
            status: "in_work",
            cancellationToken: cancellationToken);

        Assert.Equal(2, saved.Version);
        Assert.Equal("Saved", saved.Title);
        Assert.Equal("New body", saved.ContentMarkdown);
        Assert.Equal(CoreEntryTypes.ResearchNote, saved.EntryType);
        Assert.Equal("in_work", saved.Status);
    }

    [Fact]
    public async Task EntryUpdateAsync_StaleVersion_RejectsBeforeMutatingCurrentEntity()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projects = new InMemoryProjectRepository();
        var entries = new InMemoryEntryRepository();
        var project = new Project(Guid.NewGuid(), "Project", null, "general", Now);
        var current = new Entry(Guid.NewGuid(), project.Id, CoreEntryTypes.Note, "Original", null, "Body", Now);
        current.Update("Newer saved title", null, "Newer saved body", CoreEntryTypes.Note, "draft", Now.AddMinutes(1));
        projects.Seed(project);
        entries.Seed(current);
        var service = new EntryService(projects, entries, new TestClock(Now.AddMinutes(2)));

        var exception = await Assert.ThrowsAsync<OptimisticConcurrencyException>(() => service.UpdateAsync(
            current.Id,
            expectedVersion: 1,
            title: "Stale editor title",
            summary: null,
            contentMarkdown: "Stale editor body",
            cancellationToken: cancellationToken));

        Assert.Equal(nameof(Entry), exception.EntityType);
        Assert.Equal(current.Id, exception.EntityId);
        Assert.Equal(1, exception.ExpectedVersion);
        Assert.Equal(2, exception.ActualVersion);
        Assert.Equal("Newer saved title", current.Title);
        Assert.Equal("Newer saved body", current.ContentMarkdown);
        Assert.Equal(2, current.Version);
    }

    [Fact]
    public async Task EntryDeleteAsync_StaleVersion_DoesNotDeleteCurrentEntity()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projects = new InMemoryProjectRepository();
        var entries = new InMemoryEntryRepository();
        var project = new Project(Guid.NewGuid(), "Project", null, "general", Now);
        var current = new Entry(Guid.NewGuid(), project.Id, CoreEntryTypes.Note, "Entry", null, string.Empty, Now);
        current.Update("Updated elsewhere", null, string.Empty, CoreEntryTypes.Note, "draft", Now.AddMinutes(1));
        projects.Seed(project);
        entries.Seed(current);
        var service = new EntryService(projects, entries, new TestClock(Now.AddMinutes(2)));

        await Assert.ThrowsAsync<OptimisticConcurrencyException>(
            () => service.DeleteAsync(current.Id, expectedVersion: 1, cancellationToken));

        Assert.False(current.IsDeleted);
        Assert.Equal(2, current.Version);
    }

    [Fact]
    public async Task ProjectUpdateAsync_StaleVersion_RejectsBeforeMutatingCurrentEntity()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projects = new InMemoryProjectRepository();
        var current = new Project(Guid.NewGuid(), "Original", null, "general", Now);
        current.Update("Newer project name", "Newer description", "general", Now.AddMinutes(1));
        projects.Seed(current);
        var service = new ProjectService(projects, new TestClock(Now.AddMinutes(2)));

        var exception = await Assert.ThrowsAsync<OptimisticConcurrencyException>(() => service.UpdateAsync(
            current.Id,
            expectedVersion: 1,
            name: "Stale project name",
            description: "Stale description",
            profileKey: "general",
            cancellationToken: cancellationToken));

        Assert.Equal(nameof(Project), exception.EntityType);
        Assert.Equal(current.Id, exception.EntityId);
        Assert.Equal(1, exception.ExpectedVersion);
        Assert.Equal(2, exception.ActualVersion);
        Assert.Equal("Newer project name", current.Name);
        Assert.Equal("Newer description", current.Description);
        Assert.Equal(2, current.Version);
    }

    [Fact]
    public async Task ProjectArchiveAsync_StaleVersion_DoesNotArchiveCurrentEntity()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projects = new InMemoryProjectRepository();
        var current = new Project(Guid.NewGuid(), "Project", null, "general", Now);
        current.Update("Updated elsewhere", null, "general", Now.AddMinutes(1));
        projects.Seed(current);
        var service = new ProjectService(projects, new TestClock(Now.AddMinutes(2)));

        await Assert.ThrowsAsync<OptimisticConcurrencyException>(
            () => service.ArchiveAsync(current.Id, expectedVersion: 1, cancellationToken));

        Assert.False(current.IsArchived);
        Assert.Equal(2, current.Version);
    }

    [Fact]
    public async Task EntryUpdateAsync_InvalidExpectedVersion_IsRejectedExplicitly()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projects = new InMemoryProjectRepository();
        var entries = new InMemoryEntryRepository();
        var project = new Project(Guid.NewGuid(), "Project", null, "general", Now);
        var entry = new Entry(Guid.NewGuid(), project.Id, CoreEntryTypes.Note, "Entry", null, string.Empty, Now);
        projects.Seed(project);
        entries.Seed(entry);
        var service = new EntryService(projects, entries, new TestClock(Now));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.UpdateAsync(
            entry.Id,
            expectedVersion: 0,
            title: "Invalid",
            summary: null,
            contentMarkdown: string.Empty,
            cancellationToken: cancellationToken));

        Assert.Equal(1, entry.Version);
        Assert.Equal("Entry", entry.Title);
    }
}
