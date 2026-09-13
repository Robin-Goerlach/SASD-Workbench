namespace SASD.Workbench.WinForms.Tests;

/// <summary>
/// Verifies host startup parsing without creating or displaying WinForms controls.
/// </summary>
/// <remarks>
/// These tests are intentionally separate from UI automation. Their safety purpose is to ensure that
/// an acceptance/test process either uses the requested isolated data root or fails explicitly; a
/// malformed switch must never fall back silently to the normal per-user Workbench data directory.
/// </remarks>
public sealed class WorkbenchStartupOptionsTests
{
    [Fact]
    public void Parse_NoArguments_UsesNormalDataRoot()
    {
        var options = WorkbenchStartupOptions.Parse([]);

        Assert.False(options.UsesCustomDataRoot);
        Assert.Null(options.DataRootOverride);
        Assert.False(options.ShowHelp);
    }

    [Fact]
    public void Parse_DataRootAsSeparateArgument_PreservesPathWithSpaces()
    {
        const string dataRoot = @"C:\Acceptance Data\SASD Workbench";

        var options = WorkbenchStartupOptions.Parse(["--data-root", dataRoot]);

        Assert.True(options.UsesCustomDataRoot);
        Assert.Equal(dataRoot, options.DataRootOverride);
    }

    [Fact]
    public void Parse_DataRootEqualsSyntax_IsCaseInsensitive()
    {
        const string dataRoot = @"C:\Temp\Workbench-Acceptance";

        var options = WorkbenchStartupOptions.Parse([$"--DATA-ROOT={dataRoot}"]);

        Assert.True(options.UsesCustomDataRoot);
        Assert.Equal(dataRoot, options.DataRootOverride);
    }

    [Fact]
    public void Parse_DuplicateDataRoot_IsRejected()
    {
        var exception = Assert.Throws<ArgumentException>(() => WorkbenchStartupOptions.Parse(
            ["--data-root", @"C:\One", "--data-root=C:\\Two"]));

        Assert.Contains("only be supplied once", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_DataRootWithoutPath_IsRejected()
    {
        var exception = Assert.Throws<ArgumentException>(() => WorkbenchStartupOptions.Parse(["--data-root"]));

        Assert.Contains("requires a directory path", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_DataRootFollowedByAnotherSwitch_IsRejectedInsteadOfUsingSwitchAsPath()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => WorkbenchStartupOptions.Parse(["--data-root", "--help"]));

        Assert.Contains("requires a directory path", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_UnknownOption_IsRejectedInsteadOfFallingBackToNormalData()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => WorkbenchStartupOptions.Parse(["--data-rooot", @"C:\Acceptance"]));

        Assert.Contains("Unknown startup option", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--data-rooot", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("-h")]
    [InlineData("/?")]
    public void Parse_HelpSwitch_RequestsHelp(string argument)
    {
        var options = WorkbenchStartupOptions.Parse([argument]);

        Assert.True(options.ShowHelp);
        Assert.False(options.UsesCustomDataRoot);
    }

    [Fact]
    public void CreateDataPaths_CustomRoot_NormalizesPathWithoutCreatingData()
    {
        var requestedRoot = Path.Combine(
            Path.GetTempPath(),
            "SASD-Workbench-StartupOptionsTests",
            Guid.NewGuid().ToString("N"),
            "nested",
            "..");
        var options = WorkbenchStartupOptions.Parse(["--data-root", requestedRoot]);

        var paths = options.CreateDataPaths();

        Assert.Equal(Path.GetFullPath(requestedRoot), paths.RootDirectory);
        Assert.False(File.Exists(paths.DatabasePath));
    }
}
