using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Defines persistence operations required by template use cases.
/// </summary>
public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Template>> ListAsync(Guid? projectId = null, string? profileKey = null, CancellationToken cancellationToken = default);
    Task AddAsync(Template template, CancellationToken cancellationToken = default);
    Task UpdateAsync(Template template, CancellationToken cancellationToken = default);
}
