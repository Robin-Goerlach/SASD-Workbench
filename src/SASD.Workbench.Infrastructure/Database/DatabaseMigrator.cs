using System.Reflection;

namespace SASD.Workbench.Infrastructure.Database;

/// <summary>
/// Applies embedded SQL migrations exactly once and records them in schema_migrations.
/// </summary>
public sealed class DatabaseMigrator
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public DatabaseMigrator(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Applies all embedded migrations in lexical order.
    /// </summary>
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using (var bootstrap = connection.CreateCommand())
        {
            bootstrap.CommandText = """
                CREATE TABLE IF NOT EXISTS schema_migrations (
                    id TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    applied_at TEXT NOT NULL
                );
                """;
            await bootstrap.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        var assembly = typeof(DatabaseMigrator).Assembly;
        var resources = assembly.GetManifestResourceNames()
            .Where(name => name.Contains(".Database.Migrations.", StringComparison.Ordinal) && name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        foreach (var resourceName in resources)
        {
            var fileName = resourceName[(resourceName.LastIndexOf(".Migrations.", StringComparison.Ordinal) + ".Migrations.".Length)..];
            var migrationId = Path.GetFileNameWithoutExtension(fileName);

            await using var check = connection.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM schema_migrations WHERE id = $id;";
            check.Parameters.AddWithValue("$id", migrationId);
            var alreadyApplied = Convert.ToInt64(await check.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)) > 0;
            if (alreadyApplied)
            {
                continue;
            }

            await using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded migration '{resourceName}' could not be opened.");
            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using var migration = connection.CreateCommand();
            migration.Transaction = (Microsoft.Data.Sqlite.SqliteTransaction)transaction;
            migration.CommandText = sql;
            await migration.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            await using var record = connection.CreateCommand();
            record.Transaction = (Microsoft.Data.Sqlite.SqliteTransaction)transaction;
            record.CommandText = "INSERT INTO schema_migrations(id, name, applied_at) VALUES ($id, $name, $appliedAt);";
            record.Parameters.AddWithValue("$id", migrationId);
            record.Parameters.AddWithValue("$name", fileName);
            record.Parameters.AddWithValue("$appliedAt", DateTime.UtcNow.ToString("O"));
            await record.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
