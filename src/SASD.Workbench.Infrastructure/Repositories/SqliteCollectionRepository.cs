using System.Globalization;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Repositories;

/// <summary>
/// Persists hierarchical collections and many-to-many entry memberships in SQLite.
/// </summary>
public sealed class SqliteCollectionRepository : ICollectionRepository
{
    private readonly SqliteConnectionFactory _connections;

    public SqliteCollectionRepository(SqliteConnectionFactory connections)
        => _connections = connections ?? throw new ArgumentNullException(nameof(connections));

    public async Task<Collection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + " WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? ReadCollection(reader) : null;
    }

    public Task<IReadOnlyList<Collection>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        => ListAsync("c.project_id = $id", projectId, cancellationToken);

    public Task<IReadOnlyList<Collection>> ListByEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
        => ListAsync("EXISTS (SELECT 1 FROM entry_collections ec WHERE ec.collection_id = c.id AND ec.entry_id = $id)", entryId, cancellationToken);

    public async Task AddAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO collections
                (id, project_id, parent_collection_id, name, description, created_at, updated_at, sort_order, is_deleted, deleted_at)
            VALUES
                ($id, $projectId, $parentId, $name, $description, $createdAt, $updatedAt, $sortOrder, $isDeleted, $deletedAt);
            """;
        AddParameters(command, collection);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE collections
            SET parent_collection_id = $parentId, name = $name, description = $description,
                updated_at = $updatedAt, sort_order = $sortOrder, is_deleted = $isDeleted, deleted_at = $deletedAt
            WHERE id = $id;
            """;
        AddParameters(command, collection);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows != 1)
        {
            throw new InvalidOperationException($"Collection '{collection.Id}' does not exist.");
        }
    }

    public async Task AddEntryAsync(Guid collectionId, Guid entryId, DateTime createdAtUtc, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO entry_collections(entry_id, collection_id, created_at) VALUES ($entryId, $collectionId, $createdAt);";
        command.Parameters.AddWithValue("$entryId", entryId.ToString("D"));
        command.Parameters.AddWithValue("$collectionId", collectionId.ToString("D"));
        command.Parameters.AddWithValue("$createdAt", FormatUtc(createdAtUtc));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveEntryAsync(Guid collectionId, Guid entryId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM entry_collections WHERE entry_id = $entryId AND collection_id = $collectionId;";
        command.Parameters.AddWithValue("$entryId", entryId.ToString("D"));
        command.Parameters.AddWithValue("$collectionId", collectionId.ToString("D"));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<Collection>> ListAsync(string predicate, Guid id, CancellationToken cancellationToken)
    {
        var result = new List<Collection>();
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + $" WHERE {predicate} AND c.is_deleted = 0 ORDER BY c.sort_order, c.name COLLATE NOCASE;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(ReadCollection(reader));
        }
        return result;
    }

    private const string SelectColumns = """
        SELECT c.id, c.project_id, c.parent_collection_id, c.name, c.description,
               c.created_at, c.updated_at, c.sort_order, c.is_deleted, c.deleted_at
        FROM collections c
        """;

    private static void AddParameters(SqliteCommand command, Collection collection)
    {
        command.Parameters.AddWithValue("$id", collection.Id.ToString("D"));
        command.Parameters.AddWithValue("$projectId", collection.ProjectId.ToString("D"));
        command.Parameters.AddWithValue("$parentId", collection.ParentCollectionId.HasValue ? collection.ParentCollectionId.Value.ToString("D") : DBNull.Value);
        command.Parameters.AddWithValue("$name", collection.Name);
        command.Parameters.AddWithValue("$description", (object?)collection.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", FormatUtc(collection.CreatedAtUtc));
        command.Parameters.AddWithValue("$updatedAt", FormatUtc(collection.UpdatedAtUtc));
        command.Parameters.AddWithValue("$sortOrder", collection.SortOrder);
        command.Parameters.AddWithValue("$isDeleted", collection.IsDeleted ? 1 : 0);
        command.Parameters.AddWithValue("$deletedAt", collection.DeletedAtUtc.HasValue ? FormatUtc(collection.DeletedAtUtc.Value) : DBNull.Value);
    }

    private static Collection ReadCollection(SqliteDataReader reader)
        => Collection.Restore(
            Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)),
            reader.IsDBNull(2) ? null : Guid.Parse(reader.GetString(2)),
            reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4),
            ParseUtc(reader.GetString(5)), ParseUtc(reader.GetString(6)), reader.GetInt32(7),
            reader.GetInt64(8) != 0, reader.IsDBNull(9) ? null : ParseUtc(reader.GetString(9)));

    private static string FormatUtc(DateTime value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    private static DateTime ParseUtc(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
}
