using SASD.Workbench.Application.Models;

namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Defines portable project export operations independently from a concrete output format implementation.
/// </summary>
public interface IProjectExportService
{
    Task<ProjectExportResult> ExportMarkdownAsync(
        Guid projectId,
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}
