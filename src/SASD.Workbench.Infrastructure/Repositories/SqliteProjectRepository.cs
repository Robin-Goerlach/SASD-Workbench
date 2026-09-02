using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Repositories;

/// <summary>
/// Persists neutral Workbench projects in SQLite.
/// </summary>
public sealed class SqliteProjectRepository : IProjectRepository
{
    private readonly SqliteConnectionFactory _connections;

    public SqliteProjectRepository(SqliteConnectionFactory connections)
        => _connections = connections ?? throw new ArgumentNullException(nameof(connections));

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, name, description, profile_key, status, created_at, updated_at,
                   version, is_archived, is_deleted, deleted_at
            FROM projects
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", id.ToString("D"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? ReadProject(reader) : null;
    }

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<Project>();
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, name, description, profile_key, status, created_at, updated_at,
                   version, is_archived, is_deleted, deleted_at
            FROM projects
            WHERE is_deleted = 0
            ORDER BY name COLLATE NOCASE, created_at;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(ReadProject(reader));
        }

        return result;
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO projects
                (id, name, description, profile_key, status, created_at, updated_at, version,
                 is_archived, is_deleted, deleted_at)
            VALUES
                ($id, $name, $description, $profileKey, $status, $createdAt, $updatedAt, $version,
                 $isArchived, $isDeleted, $deletedAt);
            """;
        AddParameters(command, project);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE projects
            SET name = $name,
                description = $description,
                profile_key = $profileKey,
                status = $status,
                updated_at = $updatedAt,
                version = $version,
                is_archived = $isArchived,
                is_deleted = $isDeleted,
                deleted_at = $deletedAt
            WHERE id = $id AND version = $previousVersion;
            """;
        AddParameters(command, project);
        command.Parameters.AddWithValue("$previousVersion", project.Version - 1);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows != 1)
        {
            throw new DBConcurrencyException($"Project '{project.Id}' was changed or removed by another operation.");
        }
    }

    private static void AddParameters(SqliteCommand command, Project project)
    {
        command.Parameters.AddWithValue("$id", project.Id.ToString("D"));
        command.Parameters.AddWithValue("$name", project.Name);
        command.Parameters.AddWithValue("$description", (object?)project.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$profileKey", project.ProfileKey);
        command.Parameters.AddWithValue("$status", project.Status);
        command.Parameters.AddWithValue("$createdAt", FormatUtc(project.CreatedAtUtc));
        command.Parameters.AddWithValue("$updatedAt", FormatUtc(project.UpdatedAtUtc));
        command.Parameters.AddWithValue("$version", project.Version);
        command.Parameters.AddWithValue("$isArchived", project.IsArchived ? 1 : 0);
        command.Parameters.AddWithValue("$isDeleted", project.IsDeleted ? 1 : 0);
        command.Parameters.AddWithValue("$deletedAt", project.DeletedAtUtc.HasValue ? FormatUtc(project.DeletedAtUtc.Value) : DBNull.Value);
    }

    private static Project ReadProject(SqliteDataReader reader)
        => Project.Restore(
            Guid.Parse(reader.GetString(0)),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            ParseUtc(reader.GetString(5)),
            ParseUtc(reader.GetString(6)),
            reader.GetInt64(7),
            reader.GetInt64(8) != 0,
            reader.GetInt64(9) != 0,
            reader.IsDBNull(10) ? null : ParseUtc(reader.GetString(10)));

    private static string FormatUtc(DateTime value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTime ParseUtc(string value)
        => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
}
