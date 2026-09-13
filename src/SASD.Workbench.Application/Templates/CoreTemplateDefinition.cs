namespace SASD.Workbench.Application.Templates;

/// <summary>
/// Describes a profile-neutral system template that can be installed into a Workbench data store.
/// </summary>
/// <param name="Name">Human-readable template name.</param>
/// <param name="EntryType">Stable generic entry type key.</param>
/// <param name="DefaultStatus">Initial status for entries created from the template.</param>
/// <param name="ContentMarkdown">Reusable Markdown skeleton.</param>
/// <param name="Description">Short explanation of the template's generic purpose.</param>
public sealed record CoreTemplateDefinition(
    string Name,
    string EntryType,
    string DefaultStatus,
    string ContentMarkdown,
    string Description);
