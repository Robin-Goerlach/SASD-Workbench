using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Services;
using SASD.Workbench.Infrastructure.Backup;
using SASD.Workbench.Infrastructure.Configuration;
using SASD.Workbench.Infrastructure.Database;
using SASD.Workbench.Infrastructure.Export;
using SASD.Workbench.Infrastructure.FileStorage;
using SASD.Workbench.Infrastructure.Repositories;
using SASD.Workbench.Infrastructure.Time;

namespace SASD.Workbench.Infrastructure.DependencyInjection;

/// <summary>
/// Provides the canonical dependency-injection registration for the reusable SASD Workbench Core.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the complete local-first Workbench Core for a host.
    /// </summary>
    /// <param name="services">The host service collection.</param>
    /// <param name="paths">The controlled data paths used by the Workbench instance.</param>
    /// <returns>The same service collection to allow fluent host configuration.</returns>
    /// <remarks>
    /// This method deliberately contains only profile-neutral registrations. A specialized host may
    /// register its own profile services and UI components after calling this method. Core contracts
    /// use <c>TryAdd</c> where practical so tests and future hosts can replace adapters such as the clock
    /// without copying the complete composition root.
    /// </remarks>
    public static IServiceCollection AddSasdWorkbenchCore(this IServiceCollection services, WorkbenchDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(paths);

        // The host owns the choice of data root. Registering the already-created path object keeps
        // file-system policy in one place and avoids deriving different paths in individual services.
        services.TryAddSingleton(paths);
        services.TryAddSingleton(serviceProvider =>
            new SqliteConnectionFactory(serviceProvider.GetRequiredService<WorkbenchDataPaths>().DatabasePath));
        services.TryAddSingleton<DatabaseMigrator>();

        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddSingleton<IFileStorageService, LocalFileStorageService>();

        services.TryAddSingleton<IProjectRepository, SqliteProjectRepository>();
        services.TryAddSingleton<IEntryRepository, SqliteEntryRepository>();
        services.TryAddSingleton<ITemplateRepository, SqliteTemplateRepository>();
        services.TryAddSingleton<ITagRepository, SqliteTagRepository>();
        services.TryAddSingleton<IAttachmentRepository, SqliteAttachmentRepository>();
        services.TryAddSingleton<ICollectionRepository, SqliteCollectionRepository>();
        services.TryAddSingleton<IEntryLinkRepository, SqliteEntryLinkRepository>();
        services.TryAddSingleton<IActivityLogRepository, SqliteActivityLogRepository>();

        services.TryAddSingleton<ProjectService>();
        services.TryAddSingleton<EntryService>();
        services.TryAddSingleton<TemplateService>();
        services.TryAddSingleton<TagService>();
        services.TryAddSingleton<AttachmentService>();
        services.TryAddSingleton<CollectionService>();
        services.TryAddSingleton<EntryLinkService>();
        services.TryAddSingleton<ActivityLogService>();
        services.TryAddSingleton<SearchService>();

        services.TryAddSingleton<IProjectExportService, MarkdownProjectExportService>();
        services.TryAddSingleton<IBackupService, LocalBackupService>();

        return services;
    }
}
