using System.Globalization;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Repositories;

/// <summary>
/// Persists tags and entry-tag assignments in SQLite.
/// </summary>
public sealed class SqliteTagRepository : ITagRepository
{
    private readonly SqliteConnectionFactory _connections;

    public SqliteTagRepository(SqliteConnectionFactory connections)
        => _connections = connections ?? throw new ArgumentNullException(nameof(connections));

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, normalized_name, color, created_at, updated_at, is_deleted FROM tags WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? ReadTag(reader) : null;
    }

    public async Task<Tag?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedName);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, normalized_name, color, created_at, updated_at, is_deleted FROM tags WHERE normalized_name = $name;";
        command.Parameters.AddWithValue("$name", normalizedName.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? ReadTag(reader) : null;
    }

    public async Task<IReadOnlyList<Tag>> ListAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<Tag>();
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, normalized_name, color, created_at, updated_at, is_deleted FROM tags WHERE is_deleted = 0 ORDER BY name COLLATE NOCASE;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(ReadTag(reader));
        }

        return result;
    }

    public async Task<IReadOnlyList<Tag>> ListByEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        var result = new List<Tag>();
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.id, t.name, t.normalized_name, t.color, t.created_at, t.updated_at, t.is_deleted
            FROM tags t
            INNER JOIN entry_tags et ON et.tag_id = t.id
            WHERE et.entry_id = $entryId AND t.is_deleted = 0
            ORDER BY t.name COLLATE NOCASE;
            """;
        command.Parameters.AddWithValue("$entryId", entryId.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(ReadTag(reader));
        }

        return result;
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tag);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO tags(id, name, normalized_name, color, created_at, updated_at, is_deleted)
            VALUES ($id, $name, $normalizedName, $color, $createdAt, $updatedAt, $isDeleted);
            """;
        AddParameters(command, tag);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tag);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE tags
            SET name = $name, normalized_name = $normalizedName, color = $color,
                updated_at = $updatedAt, is_deleted = $isDeleted
            WHERE id = $id;
            """;
        AddParameters(command, tag);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows != 1)
        {
            throw new InvalidOperationException($"Tag '{tag.Id}' does not exist.");
        }
    }

    public async Task AttachToEntryAsync(Guid entryId, Guid tagId, DateTime createdAtUtc, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO entry_tags(entry_id, tag_id, created_at) VALUES ($entryId, $tagId, $createdAt);";
        command.Parameters.AddWithValue("$entryId", entryId.ToString("D"));
        command.Parameters.AddWithValue("$tagId", tagId.ToString("D"));
        command.Parameters.AddWithValue("$createdAt", FormatUtc(createdAtUtc));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DetachFromEntryAsync(Guid entryId, Guid tagId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM entry_tags WHERE entry_id = $entryId AND tag_id = $tagId;";
        command.Parameters.AddWithValue("$entryId", entryId.ToString("D"));
        command.Parameters.AddWithValue("$tagId", tagId.ToString("D"));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void AddParameters(SqliteCommand command, Tag tag)
    {
        command.Parameters.AddWithValue("$id", tag.Id.ToString("D"));
        command.Parameters.AddWithValue("$name", tag.Name);
        command.Parameters.AddWithValue("$normalizedName", tag.NormalizedName);
        command.Parameters.AddWithValue("$color", (object?)tag.Color ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", FormatUtc(tag.CreatedAtUtc));
        command.Parameters.AddWithValue("$updatedAt", FormatUtc(tag.UpdatedAtUtc));
        command.Parameters.AddWithValue("$isDeleted", tag.IsDeleted ? 1 : 0);
    }

    private static Tag ReadTag(SqliteDataReader reader)
        => Tag.Restore(
            Guid.Parse(reader.GetString(0)),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            ParseUtc(reader.GetString(4)),
            ParseUtc(reader.GetString(5)),
            reader.GetInt64(6) != 0);

    private static string FormatUtc(DateTime value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    private static DateTime ParseUtc(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
}
