using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Repositories;

/// <summary>
/// Persists generic Workbench entries in SQLite.
/// </summary>
public sealed class SqliteEntryRepository : IEntryRepository
{
    private readonly SqliteConnectionFactory _connections;

    public SqliteEntryRepository(SqliteConnectionFactory connections)
        => _connections = connections ?? throw new ArgumentNullException(nameof(connections));

    public async Task<Entry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, project_id, entry_type, status, title, summary, content_markdown,
                   created_at, updated_at, version, is_archived, is_deleted, deleted_at
            FROM entries
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", id.ToString("D"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? ReadEntry(reader) : null;
    }

    public async Task<IReadOnlyList<Entry>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var result = new List<Entry>();
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, project_id, entry_type, status, title, summary, content_markdown,
                   created_at, updated_at, version, is_archived, is_deleted, deleted_at
            FROM entries
            WHERE project_id = $projectId AND is_deleted = 0
            ORDER BY updated_at DESC, title COLLATE NOCASE;
            """;
        command.Parameters.AddWithValue("$projectId", projectId.ToString("D"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(ReadEntry(reader));
        }

        return result;
    }

    public async Task AddAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO entries
                (id, project_id, entry_type, status, title, summary, content_markdown,
                 created_at, updated_at, version, is_archived, is_deleted, deleted_at)
            VALUES
                ($id, $projectId, $entryType, $status, $title, $summary, $contentMarkdown,
                 $createdAt, $updatedAt, $version, $isArchived, $isDeleted, $deletedAt);
            """;
        AddParameters(command, entry);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE entries
            SET entry_type = $entryType,
                status = $status,
                title = $title,
                summary = $summary,
                content_markdown = $contentMarkdown,
                updated_at = $updatedAt,
                version = $version,
                is_archived = $isArchived,
                is_deleted = $isDeleted,
                deleted_at = $deletedAt
            WHERE id = $id AND version = $previousVersion;
            """;
        AddParameters(command, entry);
        command.Parameters.AddWithValue("$previousVersion", entry.Version - 1);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows != 1)
        {
            throw new DBConcurrencyException($"Entry '{entry.Id}' was changed or removed by another operation.");
        }
    }

    private static void AddParameters(SqliteCommand command, Entry entry)
    {
        command.Parameters.AddWithValue("$id", entry.Id.ToString("D"));
        command.Parameters.AddWithValue("$projectId", entry.ProjectId.ToString("D"));
        command.Parameters.AddWithValue("$entryType", entry.EntryType);
        command.Parameters.AddWithValue("$status", entry.Status);
        command.Parameters.AddWithValue("$title", entry.Title);
        command.Parameters.AddWithValue("$summary", (object?)entry.Summary ?? DBNull.Value);
        command.Parameters.AddWithValue("$contentMarkdown", entry.ContentMarkdown);
        command.Parameters.AddWithValue("$createdAt", FormatUtc(entry.CreatedAtUtc));
        command.Parameters.AddWithValue("$updatedAt", FormatUtc(entry.UpdatedAtUtc));
        command.Parameters.AddWithValue("$version", entry.Version);
        command.Parameters.AddWithValue("$isArchived", entry.IsArchived ? 1 : 0);
        command.Parameters.AddWithValue("$isDeleted", entry.IsDeleted ? 1 : 0);
        command.Parameters.AddWithValue("$deletedAt", entry.DeletedAtUtc.HasValue ? FormatUtc(entry.DeletedAtUtc.Value) : DBNull.Value);
    }

    private static Entry ReadEntry(SqliteDataReader reader)
        => Entry.Restore(
            Guid.Parse(reader.GetString(0)),
            Guid.Parse(reader.GetString(1)),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.GetString(6),
            ParseUtc(reader.GetString(7)),
            ParseUtc(reader.GetString(8)),
            reader.GetInt64(9),
            reader.GetInt64(10) != 0,
            reader.GetInt64(11) != 0,
            reader.IsDBNull(12) ? null : ParseUtc(reader.GetString(12)));

    private static string FormatUtc(DateTime value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTime ParseUtc(string value)
        => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
}
