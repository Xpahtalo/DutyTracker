using System;

namespace DutyTracker.Duty_Events;

/// <summary>
/// Describes a party wipe.
/// </summary>
public class WipeEvent
{
    /// <summary>
    /// The timestamp of the wipe.
    /// </summary>
    public DateTime TimeOfWipe { get; init; }

    /// <summary>
    /// How long the run lasted.
    /// </summary>
    public TimeSpan Duration { get; init; }
}
