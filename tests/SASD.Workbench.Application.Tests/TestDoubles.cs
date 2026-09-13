using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Models;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Tests;

/// <summary>
/// Minimal deterministic clock for Application tests. Keeping it explicit makes time-dependent
/// assertions readable and prevents accidental DateTime.UtcNow usage from hiding in use cases.
/// </summary>
internal sealed class TestClock(DateTime utcNow) : IClock
{
    public DateTime UtcNow { get; set; } = utcNow.Kind == DateTimeKind.Utc ? utcNow : utcNow.ToUniversalTime();
}

internal sealed class InMemoryProjectRepository : IProjectRepository
{
    private readonly Dictionary<Guid, Project> _projects = [];

    public void Seed(Project project) => _projects[project.Id] = project;

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_projects.GetValueOrDefault(id));

    public Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Project>>(_projects.Values.Where(project => !project.IsDeleted).ToArray());

    public Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        _projects.Add(project.Id, project);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _projects[project.Id] = project;
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryEntryRepository : IEntryRepository
{
    private readonly Dictionary<Guid, Entry> _entries = [];

    public IReadOnlyCollection<Entry> All => _entries.Values;

    public void Seed(Entry entry) => _entries[entry.Id] = entry;

    public Task<Entry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_entries.GetValueOrDefault(id));

    public Task<IReadOnlyList<Entry>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Entry>>(
            _entries.Values.Where(entry => entry.ProjectId == projectId && !entry.IsDeleted).ToArray());

    public Task<IReadOnlyList<Entry>> SearchAsync(EntrySearchQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<Entry> result = _entries.Values.Where(entry => !entry.IsDeleted);
        if (query.ProjectId.HasValue)
        {
            result = result.Where(entry => entry.ProjectId == query.ProjectId.Value);
        }
        if (!string.IsNullOrWhiteSpace(query.EntryType))
        {
            result = result.Where(entry => string.Equals(entry.EntryType, query.EntryType, StringComparison.Ordinal));
        }
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            result = result.Where(entry => string.Equals(entry.Status, query.Status, StringComparison.Ordinal));
        }
        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            result = result.Where(entry =>
                entry.Title.Contains(query.Text, StringComparison.OrdinalIgnoreCase)
                || (entry.Summary?.Contains(query.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                || entry.ContentMarkdown.Contains(query.Text, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<Entry>>(result.Take(query.Limit).ToArray());
    }

    public Task AddAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        _entries.Add(entry.Id, entry);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        _entries[entry.Id] = entry;
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryTemplateRepository : ITemplateRepository
{
    private readonly Dictionary<Guid, Template> _templates = [];

    public void Seed(Template template) => _templates[template.Id] = template;

    public Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_templates.GetValueOrDefault(id));

    public Task<IReadOnlyList<Template>> ListAsync(
        Guid? projectId = null,
        string? profileKey = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Template>>(
            _templates.Values.Where(template => !template.IsDeleted).ToArray());

    public Task AddAsync(Template template, CancellationToken cancellationToken = default)
    {
        _templates.Add(template.Id, template);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Template template, CancellationToken cancellationToken = default)
    {
        _templates[template.Id] = template;
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryEntryLinkRepository : IEntryLinkRepository
{
    private readonly Dictionary<Guid, EntryLink> _links = [];

    public IReadOnlyCollection<EntryLink> All => _links.Values;

    public Task<EntryLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_links.GetValueOrDefault(id));

    public Task<IReadOnlyList<EntryLink>> ListForEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<EntryLink>>(
            _links.Values.Where(link => !link.IsDeleted && (link.SourceEntryId == entryId || link.TargetEntryId == entryId)).ToArray());

    public Task AddAsync(EntryLink link, CancellationToken cancellationToken = default)
    {
        _links.Add(link.Id, link);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(EntryLink link, CancellationToken cancellationToken = default)
    {
        _links[link.Id] = link;
        return Task.CompletedTask;
    }
}
