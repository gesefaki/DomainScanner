namespace DomainScanner.Contracts.Enums;

/// <summary>
/// Defines the scheduling state of a domain monitoring job.
/// </summary>
public enum ScheduleState
{
    /// <summary>
    /// Manual scheduling - checks are only performed when explicitly triggered by user actions.
    /// </summary>
    Manual = 1,

    /// <summary>
    /// Automatic scheduling - checks are performed automatically at scheduled intervals.
    /// </summary>
    Automatic = 2,

    /// <summary>
    /// Both - checks can be performed both manually and automatically.
    /// </summary>
    Both = 3
}