using SASD.Workbench.Application.Exceptions;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Builds user-facing guidance for optimistic-concurrency conflicts in the desktop host.
/// </summary>
/// <remarks>
/// The Application exception intentionally carries technical concurrency facts, while the desktop
/// must explain the safe next action. Keeping this formatter separate from <see cref="MainForm"/>
/// makes the interaction contract testable without introducing brittle WinForms UI automation.
/// </remarks>
internal static class ConcurrencyConflictPresentation
{
    /// <summary>
    /// Creates the warning shown when an Entry save was rejected because the editor is stale.
    /// </summary>
    public static ConcurrencyConflictMessage ForEntry(OptimisticConcurrencyException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var versionDetails = exception.ActualVersion.HasValue
            ? $"You opened version {exception.ExpectedVersion}; the current saved version is {exception.ActualVersion.Value}."
            : $"The entry changed after the version you opened ({exception.ExpectedVersion}).";

        return new ConcurrencyConflictMessage(
            Status: "Save blocked: the entry changed elsewhere. Unsaved editor text was preserved.",
            Caption: "Entry changed since you opened it",
            Message: $"""
                This entry was changed by another operation after you opened it.

                Your changes were NOT saved. They are still shown in the editor, and the newer saved entry was not overwritten.

                {versionDetails}

                Before you continue, copy or otherwise preserve any editor text you still need. Then use Refresh or reselect the entry to load the current saved state, compare/merge your changes, and save again.

                Do not retry the stale save unchanged: that would risk replacing newer work after the reload.
                """);
    }
}

/// <summary>
/// Immutable text contract for a desktop concurrency warning.
/// </summary>
internal sealed record ConcurrencyConflictMessage(string Status, string Caption, string Message);
