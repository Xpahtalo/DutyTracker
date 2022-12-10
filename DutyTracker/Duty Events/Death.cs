using System;

namespace DutyTracker.Duty_Events;

public class Death
{
    public uint     PlayerId    { get; init; }
    public string   PlayerName  { get; init; }
    public DateTime TimeOfDeath { get; init; }

    public Death(uint playerId, string playerName, DateTime timeOfDeath)
    {
        PlayerId    = playerId;
        PlayerName  = playerName;
        TimeOfDeath = timeOfDeath;
    }
}
