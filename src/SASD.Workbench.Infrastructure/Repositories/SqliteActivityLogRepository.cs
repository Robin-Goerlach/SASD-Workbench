using System.Globalization;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Repositories;

/// <summary>
/// Persists lightweight activity records in chronological order.
/// </summary>
public sealed class SqliteActivityLogRepository : IActivityLogRepository
{
    private readonly SqliteConnectionFactory _connections;

    public SqliteActivityLogRepository(SqliteConnectionFactory connections)
        => _connections = connections ?? throw new ArgumentNullException(nameof(connections));

    public async Task AddAsync(ActivityLogItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO activity_log(id, project_id, entry_id, action_type, description, old_value, new_value, created_at, created_by)
            VALUES ($id, $projectId, $entryId, $actionType, $description, $oldValue, $newValue, $createdAt, $createdBy);
            """;
        command.Parameters.AddWithValue("$id", item.Id.ToString("D"));
        command.Parameters.AddWithValue("$projectId", item.ProjectId.HasValue ? item.ProjectId.Value.ToString("D") : DBNull.Value);
        command.Parameters.AddWithValue("$entryId", item.EntryId.HasValue ? item.EntryId.Value.ToString("D") : DBNull.Value);
        command.Parameters.AddWithValue("$actionType", item.ActionType);
        command.Parameters.AddWithValue("$description", item.Description);
        command.Parameters.AddWithValue("$oldValue", (object?)item.OldValue ?? DBNull.Value);
        command.Parameters.AddWithValue("$newValue", (object?)item.NewValue ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", FormatUtc(item.CreatedAtUtc));
        command.Parameters.AddWithValue("$createdBy", (object?)item.CreatedBy ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ActivityLogItem>> ListAsync(
        Guid? projectId = null,
        Guid? entryId = null,
        int limit = 200,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 5000)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 5000.");
        }

        var result = new List<ActivityLogItem>();
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, project_id, entry_id, action_type, description, old_value, new_value, created_at, created_by
            FROM activity_log
            WHERE ($projectId IS NULL OR project_id = $projectId)
              AND ($entryId IS NULL OR entry_id = $entryId)
            ORDER BY created_at DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$projectId", projectId.HasValue ? projectId.Value.ToString("D") : DBNull.Value);
        command.Parameters.AddWithValue("$entryId", entryId.HasValue ? entryId.Value.ToString("D") : DBNull.Value);
        command.Parameters.AddWithValue("$limit", limit);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(new ActivityLogItem(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(3),
                reader.GetString(4),
                ParseUtc(reader.GetString(7)),
                reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                reader.IsDBNull(2) ? null : Guid.Parse(reader.GetString(2)),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(8) ? null : reader.GetString(8)));
        }
        return result;
    }

    private static string FormatUtc(DateTime value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    private static DateTime ParseUtc(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
}
