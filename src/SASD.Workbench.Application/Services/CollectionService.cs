using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Services;

/// <summary>
/// Coordinates hierarchical collections and many-to-many entry membership.
/// </summary>
public sealed class CollectionService
{
    private readonly ICollectionRepository _collections;
    private readonly IProjectRepository _projects;
    private readonly IEntryRepository _entries;
    private readonly IClock _clock;

    public CollectionService(ICollectionRepository collections, IProjectRepository projects, IEntryRepository entries, IClock clock)
    {
        _collections = collections ?? throw new ArgumentNullException(nameof(collections));
        _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task<IReadOnlyList<Collection>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        => _collections.ListByProjectAsync(projectId, cancellationToken);

    public Task<IReadOnlyList<Collection>> ListByEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
        => _collections.ListByEntryAsync(entryId, cancellationToken);

    public async Task<Collection> CreateAsync(
        Guid projectId,
        string name,
        string? description = null,
        Guid? parentCollectionId = null,
        CancellationToken cancellationToken = default)
    {
        await RequireProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (parentCollectionId.HasValue)
        {
            await RequireParentAsync(projectId, parentCollectionId.Value, cancellationToken).ConfigureAwait(false);
        }

        var collection = new Collection(Guid.NewGuid(), projectId, name, _clock.UtcNow, parentCollectionId, description);
        await _collections.AddAsync(collection, cancellationToken).ConfigureAwait(false);
        return collection;
    }

    public async Task AddEntryAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken = default)
    {
        var collection = await RequireCollectionAsync(collectionId, cancellationToken).ConfigureAwait(false);
        var entry = await _entries.GetByIdAsync(entryId, cancellationToken).ConfigureAwait(false);
        if (entry is null || entry.IsDeleted)
        {
            throw new InvalidOperationException($"Entry '{entryId}' does not exist or is deleted.");
        }

        if (entry.ProjectId != collection.ProjectId)
        {
            throw new InvalidOperationException("An entry can only be assigned to collections in the same project.");
        }

        await _collections.AddEntryAsync(collectionId, entryId, _clock.UtcNow, cancellationToken).ConfigureAwait(false);
    }

    public Task RemoveEntryAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken = default)
        => _collections.RemoveEntryAsync(collectionId, entryId, cancellationToken);

    public async Task DeleteAsync(Guid collectionId, CancellationToken cancellationToken = default)
    {
        var collection = await RequireCollectionAsync(collectionId, cancellationToken).ConfigureAwait(false);
        collection.Delete(_clock.UtcNow);
        await _collections.UpdateAsync(collection, cancellationToken).ConfigureAwait(false);
    }

    private async Task RequireProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _projects.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
        if (project is null || project.IsDeleted)
        {
            throw new InvalidOperationException($"Project '{projectId}' does not exist or is deleted.");
        }
    }

    private async Task<Collection> RequireCollectionAsync(Guid id, CancellationToken cancellationToken)
    {
        var collection = await _collections.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (collection is null || collection.IsDeleted)
        {
            throw new InvalidOperationException($"Collection '{id}' does not exist or is deleted.");
        }
        return collection;
    }

    private async Task RequireParentAsync(Guid projectId, Guid parentId, CancellationToken cancellationToken)
    {
        var parent = await RequireCollectionAsync(parentId, cancellationToken).ConfigureAwait(false);
        if (parent.ProjectId != projectId)
        {
            throw new InvalidOperationException("A parent collection must belong to the same project.");
        }
    }
}
