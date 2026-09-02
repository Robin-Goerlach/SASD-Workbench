using SASD.Workbench.Application.Interfaces;

namespace SASD.Workbench.Infrastructure.Time;

/// <summary>
/// Production clock based on the system UTC clock.
/// </summary>
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
