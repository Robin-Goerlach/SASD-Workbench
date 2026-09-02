using System.Globalization;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Repositories;

/// <summary>
/// Persists reusable Workbench templates in SQLite.
/// </summary>
public sealed class SqliteTemplateRepository : ITemplateRepository
{
    private readonly SqliteConnectionFactory _connections;

    public SqliteTemplateRepository(SqliteConnectionFactory connections)
        => _connections = connections ?? throw new ArgumentNullException(nameof(connections));

    public async Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, project_id, profile_key, name, description, entry_type, default_status,
                   content_markdown, is_system_template, is_deleted, created_at, updated_at, sort_order
            FROM templates
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? ReadTemplate(reader) : null;
    }

    public async Task<IReadOnlyList<Template>> ListAsync(Guid? projectId = null, string? profileKey = null, CancellationToken cancellationToken = default)
    {
        var result = new List<Template>();
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, project_id, profile_key, name, description, entry_type, default_status,
                   content_markdown, is_system_template, is_deleted, created_at, updated_at, sort_order
            FROM templates
            WHERE is_deleted = 0
              AND ($projectId IS NULL OR project_id IS NULL OR project_id = $projectId)
              AND ($profileKey IS NULL OR profile_key = $profileKey)
            ORDER BY sort_order, name COLLATE NOCASE;
            """;
        command.Parameters.AddWithValue("$projectId", projectId.HasValue ? projectId.Value.ToString("D") : DBNull.Value);
        command.Parameters.AddWithValue("$profileKey", string.IsNullOrWhiteSpace(profileKey) ? DBNull.Value : profileKey.Trim());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(ReadTemplate(reader));
        }

        return result;
    }

    public async Task AddAsync(Template template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO templates
                (id, project_id, profile_key, name, description, entry_type, default_status,
                 content_markdown, is_system_template, is_deleted, created_at, updated_at, sort_order)
            VALUES
                ($id, $projectId, $profileKey, $name, $description, $entryType, $defaultStatus,
                 $contentMarkdown, $isSystemTemplate, $isDeleted, $createdAt, $updatedAt, $sortOrder);
            """;
        AddParameters(command, template);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Template template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE templates
            SET project_id = $projectId, profile_key = $profileKey, name = $name,
                description = $description, entry_type = $entryType, default_status = $defaultStatus,
                content_markdown = $contentMarkdown, is_system_template = $isSystemTemplate,
                is_deleted = $isDeleted, updated_at = $updatedAt, sort_order = $sortOrder
            WHERE id = $id;
            """;
        AddParameters(command, template);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows != 1)
        {
            throw new InvalidOperationException($"Template '{template.Id}' does not exist.");
        }
    }

    private static void AddParameters(SqliteCommand command, Template template)
    {
        command.Parameters.AddWithValue("$id", template.Id.ToString("D"));
        command.Parameters.AddWithValue("$projectId", template.ProjectId.HasValue ? template.ProjectId.Value.ToString("D") : DBNull.Value);
        command.Parameters.AddWithValue("$profileKey", template.ProfileKey);
        command.Parameters.AddWithValue("$name", template.Name);
        command.Parameters.AddWithValue("$description", (object?)template.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$entryType", template.EntryType);
        command.Parameters.AddWithValue("$defaultStatus", template.DefaultStatus);
        command.Parameters.AddWithValue("$contentMarkdown", template.ContentMarkdown);
        command.Parameters.AddWithValue("$isSystemTemplate", template.IsSystemTemplate ? 1 : 0);
        command.Parameters.AddWithValue("$isDeleted", template.IsDeleted ? 1 : 0);
        command.Parameters.AddWithValue("$createdAt", FormatUtc(template.CreatedAtUtc));
        command.Parameters.AddWithValue("$updatedAt", FormatUtc(template.UpdatedAtUtc));
        command.Parameters.AddWithValue("$sortOrder", template.SortOrder);
    }

    private static Template ReadTemplate(SqliteDataReader reader)
        => Template.Restore(
            Guid.Parse(reader.GetString(0)),
            reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
            reader.GetString(2),
            reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.GetString(5),
            reader.GetString(6),
            reader.GetString(7),
            reader.GetInt64(8) != 0,
            reader.GetInt64(9) != 0,
            ParseUtc(reader.GetString(10)),
            ParseUtc(reader.GetString(11)),
            reader.GetInt32(12));

    private static string FormatUtc(DateTime value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    private static DateTime ParseUtc(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
}
