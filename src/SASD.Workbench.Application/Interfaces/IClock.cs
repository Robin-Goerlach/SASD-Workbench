namespace SASD.Workbench.Application.Interfaces;

/// <summary>
/// Provides the current UTC time and keeps time-dependent use cases testable.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
