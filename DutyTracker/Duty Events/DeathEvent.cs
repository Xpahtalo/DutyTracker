using System;

namespace DutyTracker.Duty_Events;

/// <summary>
/// Describes a player death.
/// </summary>
public class DeathEvent
{
    /// <summary>
    /// The game Id of the player that died.
    /// </summary>
    public uint PlayerId { get; set; }

    /// <summary>
    /// The name of the player that died.
    /// </summary>
    public string PlayerName { get; set; }

    /// <summary>
    /// The time of the players death.
    /// </summary>
    public DateTime TimeOfDeath { get; set; }
}
