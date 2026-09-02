using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Services;

/// <summary>
/// Coordinates reusable template management and entry creation from templates.
/// </summary>
public sealed class TemplateService
{
    private readonly ITemplateRepository _templates;
    private readonly IProjectRepository _projects;
    private readonly IEntryRepository _entries;
    private readonly IClock _clock;

    public TemplateService(
        ITemplateRepository templates,
        IProjectRepository projects,
        IEntryRepository entries,
        IClock clock)
    {
        _templates = templates ?? throw new ArgumentNullException(nameof(templates));
        _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task<IReadOnlyList<Template>> ListAsync(Guid? projectId = null, string? profileKey = null, CancellationToken cancellationToken = default)
        => _templates.ListAsync(projectId, profileKey, cancellationToken);

    public async Task<Template> CreateAsync(
        string name,
        string entryType,
        string defaultStatus,
        string? contentMarkdown,
        Guid? projectId = null,
        string profileKey = "general",
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        if (projectId.HasValue)
        {
            await RequireProjectAsync(projectId.Value, cancellationToken).ConfigureAwait(false);
        }

        var template = new Template(
            Guid.NewGuid(),
            name,
            entryType,
            defaultStatus,
            contentMarkdown,
            _clock.UtcNow,
            projectId,
            profileKey,
            description);
        await _templates.AddAsync(template, cancellationToken).ConfigureAwait(false);
        return template;
    }

    /// <summary>
    /// Copies template defaults into a new independent Entry. Later template edits do not change the entry.
    /// </summary>
    public async Task<Entry> CreateEntryAsync(
        Guid projectId,
        Guid templateId,
        string title,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        var template = await _templates.GetByIdAsync(templateId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Template '{templateId}' does not exist.");

        if (template.IsDeleted)
        {
            throw new InvalidOperationException($"Template '{templateId}' is deleted.");
        }

        if (template.ProjectId.HasValue && template.ProjectId.Value != projectId)
        {
            throw new InvalidOperationException("The selected template belongs to another project.");
        }

        if (!string.Equals(template.ProfileKey, "general", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(template.ProfileKey, project.ProfileKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The selected template does not match the project profile.");
        }

        var entry = new Entry(
            Guid.NewGuid(),
            projectId,
            template.EntryType,
            title,
            template.Description,
            template.ContentMarkdown,
            _clock.UtcNow,
            template.DefaultStatus);
        await _entries.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        return entry;
    }

    private async Task<Project> RequireProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _projects.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project is null || project.IsDeleted)
        {
            throw new InvalidOperationException($"Project '{projectId}' does not exist or is deleted.");
        }

        return project;
    }
}
