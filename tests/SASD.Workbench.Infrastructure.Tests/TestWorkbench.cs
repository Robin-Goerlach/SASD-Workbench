using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Infrastructure.Configuration;
using SASD.Workbench.Infrastructure.Database;
using SASD.Workbench.Infrastructure.DependencyInjection;

namespace SASD.Workbench.Infrastructure.Tests;

/// <summary>
/// Owns a complete temporary Workbench composition for Infrastructure integration tests.
/// </summary>
/// <remarks>
/// Every test gets its own root directory and real SQLite database. This intentionally exercises the
/// production dependency-injection registration instead of assembling repositories differently in tests.
/// Pool clearing before cleanup is important on Windows because pooled SQLite handles can otherwise keep
/// the temporary database file locked after all logical operations have completed.
/// </remarks>
internal sealed class TestWorkbench : IDisposable
{
    private bool _disposed;

    private TestWorkbench(string rootDirectory, WorkbenchDataPaths paths, ServiceProvider services, TestClock clock)
    {
        RootDirectory = rootDirectory;
        Paths = paths;
        Services = services;
        Clock = clock;
    }

    public string RootDirectory { get; }
    public WorkbenchDataPaths Paths { get; }
    public ServiceProvider Services { get; }
    public TestClock Clock { get; }

    public T GetRequiredService<T>() where T : notnull
        => Services.GetRequiredService<T>();

    public static async Task<TestWorkbench> CreateAsync()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "SASD-Workbench-InfrastructureTests",
            Guid.NewGuid().ToString("N"));
        var paths = new WorkbenchDataPaths(root);
        paths.EnsureDirectories();

        var clock = new TestClock(new DateTime(2026, 9, 13, 18, 0, 0, DateTimeKind.Utc));
        var registrations = new ServiceCollection();
        registrations.AddSingleton<IClock>(clock);
        registrations.AddSasdWorkbenchCore(paths);
        var provider = registrations.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        try
        {
            var migrator = provider.GetRequiredService<DatabaseMigrator>();
            await migrator.MigrateAsync().ConfigureAwait(false);
            return new TestWorkbench(root, paths, provider, clock);
        }
        catch
        {
            provider.Dispose();
            SqliteConnection.ClearAllPools();
            TryDeleteDirectory(root);
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Services.Dispose();
        SqliteConnection.ClearAllPools();
        TryDeleteDirectory(RootDirectory);
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
            // Cleanup failure must not hide the actual assertion result. Each test uses a unique temp root,
            // so a rare external file-lock race cannot contaminate a subsequent test run.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    internal sealed class TestClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow { get; private set; } = utcNow.Kind == DateTimeKind.Utc ? utcNow : utcNow.ToUniversalTime();

        public void Advance(TimeSpan amount) => UtcNow = UtcNow.Add(amount);
    }
}
