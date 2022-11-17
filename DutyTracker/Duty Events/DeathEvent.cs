using System;

namespace DutyTracker.Duty_Events;

/// <summary>
/// Describes a player death.
/// </summary>
public class DeathEvent
{
    public DeathEvent(uint playerId, string playerName, DateTime timeOfDeath)
    {
        PlayerId    = playerId;
        PlayerName  = playerName;
        TimeOfDeath = timeOfDeath;
    }

    /// <summary>
    /// The game Id of the player that died.
    /// </summary>
    public uint PlayerId { get; init; }

    /// <summary>
    /// The name of the player that died.
    /// </summary>
    public string PlayerName { get; init; }

    /// <summary>
    /// The time of the players death.
    /// </summary>
    public DateTime TimeOfDeath { get; init; }
}
