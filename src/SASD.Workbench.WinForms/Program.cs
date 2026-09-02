using SASD.Workbench.Application.Services;
using SASD.Workbench.Infrastructure.Configuration;
using SASD.Workbench.Infrastructure.Database;
using SASD.Workbench.Infrastructure.Repositories;
using SASD.Workbench.Infrastructure.Time;

namespace SASD.Workbench.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var paths = WorkbenchDataPaths.CreateDefault();
            paths.EnsureDirectories();

            var connections = new SqliteConnectionFactory(paths.DatabasePath);
            var migrator = new DatabaseMigrator(connections);
            migrator.MigrateAsync().GetAwaiter().GetResult();

            var clock = new SystemClock();
            var projectRepository = new SqliteProjectRepository(connections);
            var entryRepository = new SqliteEntryRepository(connections);
            var projectService = new ProjectService(projectRepository, clock);
            var entryService = new EntryService(projectRepository, entryRepository, clock);

            Application.Run(new MainForm(projectService, entryService, paths));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"SASD Workbench could not be started.\n\n{ex.Message}",
                "SASD Workbench",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
