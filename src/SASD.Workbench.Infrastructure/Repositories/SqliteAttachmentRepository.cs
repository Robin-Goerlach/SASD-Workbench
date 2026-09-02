using System.Globalization;
using Microsoft.Data.Sqlite;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Infrastructure.Database;

namespace SASD.Workbench.Infrastructure.Repositories;

/// <summary>
/// Persists attachment metadata in SQLite while file bytes remain in controlled storage.
/// </summary>
public sealed class SqliteAttachmentRepository : IAttachmentRepository
{
    private readonly SqliteConnectionFactory _connections;

    public SqliteAttachmentRepository(SqliteConnectionFactory connections)
        => _connections = connections ?? throw new ArgumentNullException(nameof(connections));

    public async Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + " WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? ReadAttachment(reader) : null;
    }

    public async Task<IReadOnlyList<Attachment>> ListByEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        var result = new List<Attachment>();
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = SelectColumns + " WHERE entry_id = $entryId AND is_deleted = 0 ORDER BY created_at, original_file_name COLLATE NOCASE;";
        command.Parameters.AddWithValue("$entryId", entryId.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(ReadAttachment(reader));
        }

        return result;
    }

    public async Task AddAsync(Attachment attachment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO attachments
                (id, entry_id, original_file_name, stored_file_name, relative_path, mime_type,
                 file_extension, file_size, sha256_hash, comment, created_at, updated_at, is_deleted, deleted_at)
            VALUES
                ($id, $entryId, $originalFileName, $storedFileName, $relativePath, $mimeType,
                 $fileExtension, $fileSize, $sha256Hash, $comment, $createdAt, $updatedAt, $isDeleted, $deletedAt);
            """;
        AddParameters(command, attachment);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Attachment attachment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);
        await using var connection = await _connections.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE attachments
            SET comment = $comment, updated_at = $updatedAt, is_deleted = $isDeleted, deleted_at = $deletedAt
            WHERE id = $id;
            """;
        AddParameters(command, attachment);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows != 1)
        {
            throw new InvalidOperationException($"Attachment '{attachment.Id}' does not exist.");
        }
    }

    private const string SelectColumns = """
        SELECT id, entry_id, original_file_name, stored_file_name, relative_path, mime_type,
               file_extension, file_size, sha256_hash, comment, created_at, updated_at, is_deleted, deleted_at
        FROM attachments
        """;

    private static void AddParameters(SqliteCommand command, Attachment attachment)
    {
        command.Parameters.AddWithValue("$id", attachment.Id.ToString("D"));
        command.Parameters.AddWithValue("$entryId", attachment.EntryId.ToString("D"));
        command.Parameters.AddWithValue("$originalFileName", attachment.OriginalFileName);
        command.Parameters.AddWithValue("$storedFileName", attachment.StoredFileName);
        command.Parameters.AddWithValue("$relativePath", attachment.RelativePath);
        command.Parameters.AddWithValue("$mimeType", (object?)attachment.MimeType ?? DBNull.Value);
        command.Parameters.AddWithValue("$fileExtension", (object?)attachment.FileExtension ?? DBNull.Value);
        command.Parameters.AddWithValue("$fileSize", attachment.FileSize);
        command.Parameters.AddWithValue("$sha256Hash", attachment.Sha256Hash);
        command.Parameters.AddWithValue("$comment", (object?)attachment.Comment ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", FormatUtc(attachment.CreatedAtUtc));
        command.Parameters.AddWithValue("$updatedAt", FormatUtc(attachment.UpdatedAtUtc));
        command.Parameters.AddWithValue("$isDeleted", attachment.IsDeleted ? 1 : 0);
        command.Parameters.AddWithValue("$deletedAt", attachment.DeletedAtUtc.HasValue ? FormatUtc(attachment.DeletedAtUtc.Value) : DBNull.Value);
    }

    private static Attachment ReadAttachment(SqliteDataReader reader)
        => Attachment.Restore(
            Guid.Parse(reader.GetString(0)),
            Guid.Parse(reader.GetString(1)),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.GetInt64(7),
            reader.GetString(8),
            reader.IsDBNull(9) ? null : reader.GetString(9),
            ParseUtc(reader.GetString(10)),
            ParseUtc(reader.GetString(11)),
            reader.GetInt64(12) != 0,
            reader.IsDBNull(13) ? null : ParseUtc(reader.GetString(13)));

    private static string FormatUtc(DateTime value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    private static DateTime ParseUtc(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
}
