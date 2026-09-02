namespace SASD.Workbench.Infrastructure.Configuration;

/// <summary>
/// Resolves the local data directories used by the Workbench desktop host.
/// </summary>
public sealed class WorkbenchDataPaths
{
    public WorkbenchDataPaths(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        RootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string RootDirectory { get; }
    public string DatabasePath => Path.Combine(RootDirectory, "workbench.db");
    public string AttachmentsDirectory => Path.Combine(RootDirectory, "attachments");
    public string BackupsDirectory => Path.Combine(RootDirectory, "backups");
    public string ExportsDirectory => Path.Combine(RootDirectory, "exports");
    public string LogsDirectory => Path.Combine(RootDirectory, "logs");

    /// <summary>
    /// Creates the directories that are safe to provision at application startup.
    /// </summary>
    public void EnsureDirectories()
    {
        Directory.CreateDirectory(RootDirectory);
        Directory.CreateDirectory(AttachmentsDirectory);
        Directory.CreateDirectory(BackupsDirectory);
        Directory.CreateDirectory(ExportsDirectory);
        Directory.CreateDirectory(LogsDirectory);
    }

    /// <summary>
    /// Returns the default per-user Windows data location.
    /// </summary>
    public static WorkbenchDataPaths CreateDefault()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            throw new InvalidOperationException("The local application data directory could not be resolved.");
        }

        return new WorkbenchDataPaths(Path.Combine(localAppData, "SASD", "Workbench"));
    }
}
