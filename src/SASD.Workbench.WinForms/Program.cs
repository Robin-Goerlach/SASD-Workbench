using Microsoft.Extensions.DependencyInjection;
using SASD.Workbench.Infrastructure.Configuration;
using SASD.Workbench.Infrastructure.Database;
using SASD.Workbench.Infrastructure.DependencyInjection;

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
