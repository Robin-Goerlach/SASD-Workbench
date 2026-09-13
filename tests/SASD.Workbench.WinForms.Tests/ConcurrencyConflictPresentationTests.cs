using SASD.Workbench.Application.Exceptions;

namespace SASD.Workbench.WinForms.Tests;

/// <summary>
/// Protects the non-visual interaction contract for stale editor saves.
/// </summary>
/// <remarks>
/// The actual MessageBox remains a WinForms concern and is covered by the manual desktop acceptance
/// pass. These tests deliberately verify the safety-critical wording separately: a conflict must make
/// clear that the save was rejected, the newer persisted state was not overwritten, and the user must
/// preserve/merge the still-visible editor text before reloading.
/// </remarks>
public sealed class ConcurrencyConflictPresentationTests
{
    [Fact]
    public void ForEntry_WithKnownCurrentVersion_ExplainsSafeRecoveryWithoutSuggestingRetry()
    {
        var exception = new OptimisticConcurrencyException(
            "Entry",
            Guid.NewGuid(),
            expectedVersion: 3,
            actualVersion: 4);

        var message = ConcurrencyConflictPresentation.ForEntry(exception);

        Assert.Contains("Save blocked", message.Status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("preserved", message.Status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("NOT saved", message.Message, StringComparison.Ordinal);
        Assert.Contains("not overwritten", message.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("still shown in the editor", message.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("version 3", message.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("version is 4", message.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Refresh", message.Message, StringComparison.Ordinal);
        Assert.Contains("compare/merge", message.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Do not retry", message.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ForEntry_WhenRepositoryRaceCannotReportActualVersion_StillGivesSafeGuidance()
    {
        var exception = new OptimisticConcurrencyException(
            "Entry",
            Guid.NewGuid(),
            expectedVersion: 7);

        var message = ConcurrencyConflictPresentation.ForEntry(exception);

        Assert.Contains("changed after the version you opened (7)", message.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("NOT saved", message.Message, StringComparison.Ordinal);
        Assert.Contains("newer saved entry was not overwritten", message.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("preserve", message.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ForEntry_NullException_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => ConcurrencyConflictPresentation.ForEntry(null!));
    }
}
