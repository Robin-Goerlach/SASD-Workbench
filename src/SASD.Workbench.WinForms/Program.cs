using Microsoft.Extensions.DependencyInjection;
using SASD.Workbench.Infrastructure.Database;
using SASD.Workbench.Infrastructure.DependencyInjection;

namespace SASD.Workbench.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var startupOptions = WorkbenchStartupOptions.Parse(args);
            if (startupOptions.ShowHelp)
            {
                MessageBox.Show(
                    WorkbenchStartupOptions.HelpText,
                    "SASD Workbench – Startup options",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // An explicit --data-root is resolved before directories, migrations or services are
            // touched. Therefore a malformed/unknown option cannot accidentally initialize the normal
            // per-user data directory after the caller intended to start an isolated acceptance instance.
            var paths = startupOptions.CreateDataPaths();
            paths.EnsureDirectories();

            var services = new ServiceCollection();
            services.AddSasdWorkbenchCore(paths);
            services.AddSingleton<MainForm>();

            // Validate the complete composition root before showing any UI. A missing registration
            // should fail at startup with one clear error instead of surfacing later in a button click.
            using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            var migrator = serviceProvider.GetRequiredService<DatabaseMigrator>();
            migrator.MigrateAsync().GetAwaiter().GetResult();

            System.Windows.Forms.Application.Run(serviceProvider.GetRequiredService<MainForm>());
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
