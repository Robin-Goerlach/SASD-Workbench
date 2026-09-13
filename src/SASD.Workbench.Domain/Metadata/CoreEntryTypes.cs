namespace SASD.Workbench.Domain.Metadata;

/// <summary>
/// Defines profile-neutral entry type keys that are useful across the Workbench product family.
/// </summary>
/// <remarks>
/// The catalog is intentionally open. Profiles may define additional entry types without changing
/// the Core. These constants only prevent every host from inventing different keys for concepts that
/// are already common to research, engineering, administration and general knowledge work.
/// </remarks>
public static class CoreEntryTypes
{
    public const string Note = "note";
    public const string ResearchNote = "research_note";
    public const string ResearchQuestion = "research_question";
    public const string ResearchSource = "research_source";
    public const string Observation = "observation";
    public const string Hypothesis = "hypothesis";
    public const string Finding = "finding";
    public const string Conclusion = "conclusion";

    private static readonly string[] BuiltInValues =
    [
        Note,
        ResearchNote,
        ResearchQuestion,
        ResearchSource,
        Observation,
        Hypothesis,
        Finding,
        Conclusion
    ];

    /// <summary>
    /// Gets the stable V1 profile-neutral entry type keys supplied by the Core.
    /// </summary>
    public static IReadOnlyList<string> BuiltIn => BuiltInValues;

    /// <summary>
    /// Returns whether the supplied value is one of the Core's built-in entry type keys.
    /// </summary>
    public static bool IsBuiltIn(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && BuiltInValues.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
}
