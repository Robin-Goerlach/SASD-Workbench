using SASD.Workbench.Infrastructure.Configuration;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Represents host-only startup options for the first WinForms shell.
/// </summary>
/// <remarks>
/// Command-line parsing deliberately stays in the host instead of being added to the shared Core.
/// Future hosts may have different configuration surfaces while still consuming the same
/// <see cref="WorkbenchDataPaths"/> abstraction and <c>AddSasdWorkbenchCore(...)</c> composition root.
/// </remarks>
internal sealed record WorkbenchStartupOptions(string? DataRootOverride, bool ShowHelp)
{
    public bool UsesCustomDataRoot => !string.IsNullOrWhiteSpace(DataRootOverride);

    /// <summary>
    /// Creates the data-path abstraction selected for this process.
    /// </summary>
    public WorkbenchDataPaths CreateDataPaths()
        => UsesCustomDataRoot
            ? new WorkbenchDataPaths(DataRootOverride!)
            : WorkbenchDataPaths.CreateDefault();

    /// <summary>
    /// Parses the intentionally small V1 desktop command line.
    /// </summary>
    /// <remarks>
    /// Unknown switches fail instead of being ignored. A typo in <c>--data-root</c> must never make an
    /// acceptance/test instance silently fall back to the user's normal Workbench data directory.
    /// </remarks>
    public static WorkbenchStartupOptions Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        string? dataRoot = null;
        var showHelp = false;

        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];
            ArgumentException.ThrowIfNullOrWhiteSpace(argument, nameof(args));

            if (IsHelpArgument(argument))
            {
                showHelp = true;
                continue;
            }

            if (argument.Equals("--data-root", StringComparison.OrdinalIgnoreCase))
            {
                if (dataRoot is not null)
                {
                    throw new ArgumentException("The --data-root option may only be supplied once.", nameof(args));
                }

                if (index + 1 >= args.Count || IsSwitch(args[index + 1]))
                {
                    throw new ArgumentException("The --data-root option requires a directory path.", nameof(args));
                }

                dataRoot = NormalizeDataRoot(args[++index]);
                continue;
            }

            const string dataRootPrefix = "--data-root=";
            if (argument.StartsWith(dataRootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (dataRoot is not null)
                {
                    throw new ArgumentException("The --data-root option may only be supplied once.", nameof(args));
                }

                dataRoot = NormalizeDataRoot(argument[dataRootPrefix.Length..]);
                continue;
            }

            throw new ArgumentException(
                $"Unknown startup option '{argument}'. Use --help to display the supported options.",
                nameof(args));
        }

        return new WorkbenchStartupOptions(dataRoot, showHelp);
    }

    public static string HelpText => """
        SASD Workbench startup options

        --data-root <path>
        --data-root=<path>
            Use an explicit Workbench data directory for this process.
            This is recommended for acceptance tests, experiments and isolated test data.

        --help, -h, /?
            Show this help.

        Without --data-root the normal per-user Workbench data directory is used.
        """;

    private static string NormalizeDataRoot(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        return value.Trim();
    }

    private static bool IsHelpArgument(string value)
        => value.Equals("--help", StringComparison.OrdinalIgnoreCase)
            || value.Equals("-h", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/?", StringComparison.OrdinalIgnoreCase);

    private static bool IsSwitch(string value)
        => value.StartsWith("--", StringComparison.Ordinal) || IsHelpArgument(value);
}
