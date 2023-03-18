using System;
using DutyTracker.Enums;

namespace DutyTracker.Duty_Events;

public class Death
{
    public uint     PlayerId    { get; init; }
    public string   PlayerName  { get; init; }
    public DateTime TimeOfDeath { get; init; }
    public Alliance Alliance    { get; init; }

    public Death(uint playerId, string playerName, DateTime timeOfDeath, Alliance alliance)
    {
        PlayerId    = playerId;
        PlayerName  = playerName;
        TimeOfDeath = timeOfDeath;
        Alliance    = alliance;
    }
}
