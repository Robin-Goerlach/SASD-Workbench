using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Domain.Metadata;

namespace SASD.Workbench.Application.Tests;

/// <summary>
/// Verifies relation use-case rules independently from SQLite. The Core deliberately permits custom
/// relation keys, but V1 relations must stay within one project and must never target deleted entries.
/// </summary>
public sealed class EntryLinkServiceTests
{
    private static readonly DateTime Now = new(2026, 9, 13, 17, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateAsync_SameProject_PersistsNormalizedRelation()
    {
        var entries = new InMemoryEntryRepository();
        var links = new InMemoryEntryLinkRepository();
        var projectId = Guid.NewGuid();
        var source = CreateEntry(projectId, "Source");
        var target = CreateEntry(projectId, "Target");
        entries.Seed(source);
        entries.Seed(target);
        var service = new EntryLinkService(links, entries, new TestClock(Now));

        var link = await service.CreateAsync(source.Id, target.Id, " Custom_Relation ", "  Evidence  ", "tester");

        Assert.Equal(source.Id, link.SourceEntryId);
        Assert.Equal(target.Id, link.TargetEntryId);
        Assert.Equal("custom_relation", link.RelationType);
        Assert.Equal("Evidence", link.Comment);
        Assert.Equal("tester", link.CreatedBy);
        Assert.Equal(Now, link.CreatedAtUtc);
        Assert.Contains(links.All, candidate => candidate.Id == link.Id);
    }

    [Fact]
    public async Task CreateAsync_CrossProject_RejectsWithoutWritingLink()
    {
        var entries = new InMemoryEntryRepository();
        var links = new InMemoryEntryLinkRepository();
        var source = CreateEntry(Guid.NewGuid(), "Source");
        var target = CreateEntry(Guid.NewGuid(), "Target");
        entries.Seed(source);
        entries.Seed(target);
        var service = new EntryLinkService(links, entries, new TestClock(Now));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(source.Id, target.Id, EntryRelationTypes.RelatedTo));

        Assert.True(exception.Message.Contains("same project", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(links.All);
    }

    [Fact]
    public async Task CreateAsync_DeletedEntry_RejectsWithoutWritingLink()
    {
        var entries = new InMemoryEntryRepository();
        var links = new InMemoryEntryLinkRepository();
        var projectId = Guid.NewGuid();
        var source = CreateEntry(projectId, "Source");
        var target = CreateEntry(projectId, "Target");
        target.Delete(Now);
        entries.Seed(source);
        entries.Seed(target);
        var service = new EntryLinkService(links, entries, new TestClock(Now));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(source.Id, target.Id, EntryRelationTypes.Supports));
        Assert.Empty(links.All);
    }

    [Fact]
    public async Task CreateAsync_MissingEntry_RejectsWithoutWritingLink()
    {
        var entries = new InMemoryEntryRepository();
        var links = new InMemoryEntryLinkRepository();
        var source = CreateEntry(Guid.NewGuid(), "Source");
        entries.Seed(source);
        var service = new EntryLinkService(links, entries, new TestClock(Now));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(source.Id, Guid.NewGuid(), EntryRelationTypes.References));
        Assert.Empty(links.All);
    }

    [Fact]
    public async Task DeleteAsync_MarksLinkDeleted_AndSecondDeleteIsNoOp()
    {
        var entries = new InMemoryEntryRepository();
        var links = new CountingEntryLinkRepository();
        var projectId = Guid.NewGuid();
        var source = CreateEntry(projectId, "Source");
        var target = CreateEntry(projectId, "Target");
        entries.Seed(source);
        entries.Seed(target);
        var service = new EntryLinkService(links, entries, new TestClock(Now));
        var link = await service.CreateAsync(source.Id, target.Id, EntryRelationTypes.Supports);

        await service.DeleteAsync(link.Id);
        await service.DeleteAsync(link.Id);

        Assert.True(link.IsDeleted);
        Assert.Equal(1, links.UpdateCount);
    }

    private static Entry CreateEntry(Guid projectId, string title)
        => new(Guid.NewGuid(), projectId, CoreEntryTypes.Note, title, null, string.Empty, Now);

    private sealed class CountingEntryLinkRepository : SASD.Workbench.Application.Interfaces.IEntryLinkRepository
    {
        private readonly Dictionary<Guid, EntryLink> _links = [];

        public int UpdateCount { get; private set; }

        public Task<EntryLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_links.GetValueOrDefault(id));

        public Task<IReadOnlyList<EntryLink>> ListForEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<EntryLink>>(
                _links.Values.Where(link => link.SourceEntryId == entryId || link.TargetEntryId == entryId).ToArray());

        public Task AddAsync(EntryLink link, CancellationToken cancellationToken = default)
        {
            _links.Add(link.Id, link);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(EntryLink link, CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            _links[link.Id] = link;
            return Task.CompletedTask;
        }
    }
}
