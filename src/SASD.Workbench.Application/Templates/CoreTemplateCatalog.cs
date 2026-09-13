using SASD.Workbench.Domain.Metadata;

namespace SASD.Workbench.Application.Templates;

/// <summary>
/// Supplies the small profile-neutral V1 system-template set shared by Workbench products.
/// </summary>
public static class CoreTemplateCatalog
{
    private static readonly CoreTemplateDefinition[] Definitions =
    [
        new(
            "General note",
            CoreEntryTypes.Note,
            "draft",
            """
            # Note

            ## Summary

            ## Details

            ## Next steps
            """,
            "General-purpose note without profile-specific semantics."),
        new(
            "Research note",
            CoreEntryTypes.ResearchNote,
            "draft",
            """
            # Research note

            ## Topic

            ## Sources

            ## Notes

            ## Findings

            ## Open questions
            """,
            "General research note for collecting sources, notes and interim findings."),
        new(
            "Research question",
            CoreEntryTypes.ResearchQuestion,
            "open",
            """
            # Research question

            ## Question

            ## Scope

            ## Sources

            ## Observations

            ## Hypotheses

            ## Interim conclusions

            ## Result
            """,
            "Tracks a question from initial scope through evidence and result."),
        new(
            "Research source",
            CoreEntryTypes.ResearchSource,
            "active",
            """
            # Research source

            ## Citation / bibliographic data

            ## Summary

            ## Relevant passages or facts

            ## Reliability notes

            ## Related questions
            """,
            "Captures a generic source and the information extracted from it."),
        new(
            "Observation",
            CoreEntryTypes.Observation,
            "recorded",
            """
            # Observation

            ## Context

            ## What was observed

            ## Evidence / source

            ## Notes
            """,
            "Records an observation separately from its interpretation."),
        new(
            "Hypothesis",
            CoreEntryTypes.Hypothesis,
            "proposed",
            """
            # Hypothesis

            ## Statement

            ## Rationale

            ## Supporting evidence

            ## Contradicting evidence

            ## Test / next step

            ## Status notes
            """,
            "Records an interpretation or explanation that can be supported, contradicted or refuted."),
        new(
            "Finding",
            CoreEntryTypes.Finding,
            "draft",
            """
            # Finding

            ## Finding

            ## Evidence

            ## Limitations

            ## Related entries
            """,
            "Captures an evidence-backed intermediate research or engineering finding."),
        new(
            "Conclusion",
            CoreEntryTypes.Conclusion,
            "draft",
            """
            # Conclusion

            ## Conclusion

            ## Basis

            ## Remaining uncertainty

            ## Follow-up
            """,
            "Captures a current conclusion while keeping its basis and remaining uncertainty visible.")
    ];

    /// <summary>
    /// Gets the immutable set of system-template definitions supplied by the Core.
    /// </summary>
    public static IReadOnlyList<CoreTemplateDefinition> All => Definitions;
}
