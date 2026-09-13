using System.Globalization;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Repositories;

/// <summary>
/// Persists semantic links between entries in SQLite.
/// </summary>
public sealed class SqliteEntryLinkRepository : IEntryLinkRepository
{
    private readonly SqliteConnectionFactory _connections;

    public SqliteEntryLinkRepository(SqliteConnectionFactory connections)
        => _connections = connections ?? throw new ArgumentNullException(nameof(connections));

    public async Task<EntryLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + " WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? ReadLink(reader) : null;
    }

    public async Task<IReadOnlyList<EntryLink>> ListForEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        var result = new List<EntryLink>();
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + " WHERE (source_entry_id = $entryId OR target_entry_id = $entryId) AND is_deleted = 0 ORDER BY created_at;";
        command.Parameters.AddWithValue("$entryId", entryId.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(ReadLink(reader));
        }
        return result;
    }

    public async Task AddAsync(EntryLink link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO entry_links(id, source_entry_id, target_entry_id, relation_type, comment, created_at, created_by, is_deleted)
            VALUES ($id, $sourceId, $targetId, $relationType, $comment, $createdAt, $createdBy, $isDeleted);
            """;
        AddParameters(command, link);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(EntryLink link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE entry_links SET is_deleted = $isDeleted WHERE id = $id;";
        AddParameters(command, link);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows != 1)
        {
            throw new InvalidOperationException($"Entry link '{link.Id}' does not exist.");
        }
    }

    private const string SelectColumns = """
        SELECT id, source_entry_id, target_entry_id, relation_type, comment, created_at, created_by, is_deleted
        FROM entry_links
        """;

    private static void AddParameters(SqliteCommand command, EntryLink link)
    {
        command.Parameters.AddWithValue("$id", link.Id.ToString("D"));
        command.Parameters.AddWithValue("$sourceId", link.SourceEntryId.ToString("D"));
        command.Parameters.AddWithValue("$targetId", link.TargetEntryId.ToString("D"));
        command.Parameters.AddWithValue("$relationType", link.RelationType);
        command.Parameters.AddWithValue("$comment", (object?)link.Comment ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", FormatUtc(link.CreatedAtUtc));
        command.Parameters.AddWithValue("$createdBy", (object?)link.CreatedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("$isDeleted", link.IsDeleted ? 1 : 0);
    }

    private static EntryLink ReadLink(SqliteDataReader reader)
        => EntryLink.Restore(
            Guid.Parse(reader.GetString(0)), Guid.Parse(reader.GetString(1)), Guid.Parse(reader.GetString(2)),
            reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4), ParseUtc(reader.GetString(5)),
            reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetInt64(7) != 0);

    private static string FormatUtc(DateTime value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    private static DateTime ParseUtc(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
}
