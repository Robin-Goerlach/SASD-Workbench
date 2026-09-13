using System.Globalization;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Infrastructure.Activity;

/// <summary>
/// Writes lightweight Core activity records on an existing SQLite connection and transaction.
/// </summary>
/// <remarks>
/// Repositories use this writer only after the primary mutation has succeeded. Because the caller
/// supplies the same transaction used for the domain write, the database mutation and its activity
/// record commit or roll back together. This is deliberately a lightweight history mechanism, not a
/// tamper-evident or regulatory audit trail.
///
/// The type is public only because the SQLite repository constructors are public for dependency
/// injection. It remains an Infrastructure implementation detail; Workbench hosts should register the
/// Core through <c>AddSasdWorkbenchCore(...)</c> instead of resolving this writer directly.
/// </remarks>
public sealed class SqliteActivityWriter
{
    private readonly IClock _clock;

    public SqliteActivityWriter(IClock clock)
        => _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>
    /// Writes one activity record inside the caller's transaction.
    /// </summary>
    public async Task WriteAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string actionType,
        string description,
        Guid? projectId = null,
        Guid? entryId = null,
        string? oldValue = null,
        string? newValue = null,
        string? createdBy = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);

        // Constructing the domain object here keeps the same validation rules for automatic and
        // explicitly recorded activities instead of duplicating length/null validation in SQL code.
        var item = new ActivityLogItem(
            Guid.NewGuid(),
            actionType,
            description,
            _clock.UtcNow,
            projectId,
            entryId,
            oldValue,
            newValue,
            createdBy);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO activity_log
                (id, project_id, entry_id, action_type, description, old_value, new_value, created_at, created_by)
            VALUES
                ($id, $projectId, $entryId, $actionType, $description, $oldValue, $newValue, $createdAt, $createdBy);
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

    /// <summary>
    /// Resolves an entry's owning project while staying inside the caller's transaction.
    /// </summary>
    public async Task<Guid> RequireProjectIdForEntryAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid entryId,
        CancellationToken cancellationToken = default)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT project_id FROM entries WHERE id = $entryId;";
        command.Parameters.AddWithValue("$entryId", entryId.ToString("D"));
        var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (value is null || value is DBNull)
        {
            throw new InvalidOperationException($"Entry '{entryId}' does not exist while recording activity.");
        }

        return Guid.Parse(Convert.ToString(value, CultureInfo.InvariantCulture)
            ?? throw new InvalidOperationException("Entry project id could not be read."));
    }

    /// <summary>
    /// Resolves a collection's owning project while staying inside the caller's transaction.
    /// </summary>
    public async Task<Guid> RequireProjectIdForCollectionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid collectionId,
        CancellationToken cancellationToken = default)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT project_id FROM collections WHERE id = $collectionId;";
        command.Parameters.AddWithValue("$collectionId", collectionId.ToString("D"));
        var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (value is null || value is DBNull)
        {
            throw new InvalidOperationException($"Collection '{collectionId}' does not exist while recording activity.");
        }

        return Guid.Parse(Convert.ToString(value, CultureInfo.InvariantCulture)
            ?? throw new InvalidOperationException("Collection project id could not be read."));
    }

    private static string FormatUtc(DateTime value)
        => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
}
