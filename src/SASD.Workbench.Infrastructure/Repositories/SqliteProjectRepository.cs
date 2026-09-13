using System.Globalization;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Exceptions;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Domain.Metadata;
using SASD.Workbench.Infrastructure.Activity;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Repositories;

/// <summary>
/// Persists neutral Workbench projects in SQLite.
/// </summary>
public sealed class SqliteProjectRepository : IProjectRepository
{
    private readonly SqliteConnectionFactory _connections;
    private readonly SqliteActivityWriter _activity;

    public SqliteProjectRepository(SqliteConnectionFactory connections, SqliteActivityWriter activity)
    {
        _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        _activity = activity ?? throw new ArgumentNullException(nameof(activity));
    }

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
        using var transaction = connection.BeginTransaction();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
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

        await _activity.WriteAsync(
            connection,
            transaction,
            CoreActivityTypes.ProjectCreated,
            $"Created project '{project.Name}'.",
            projectId: project.Id,
            newValue: project.Status,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        transaction.Commit();
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
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
        var expectedVersion = project.Version - 1;
        command.Parameters.AddWithValue("$previousVersion", expectedVersion);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows != 1)
        {
            throw new OptimisticConcurrencyException(nameof(Project), project.Id, expectedVersion);
        }

        var actionType = project.IsDeleted
            ? CoreActivityTypes.ProjectDeleted
            : project.IsArchived
                ? CoreActivityTypes.ProjectArchived
                : CoreActivityTypes.ProjectUpdated;

        await _activity.WriteAsync(
            connection,
            transaction,
            actionType,
            $"Persisted project '{project.Name}' with status '{project.Status}'.",
            projectId: project.Id,
            newValue: project.Status,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        transaction.Commit();
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
