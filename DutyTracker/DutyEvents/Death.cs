using System;
using DutyTracker.Enums;

namespace DutyTracker.DutyEvents;

public class Death(string playerName, DateTime timeOfDeath, Alliance alliance)
{
    public readonly string PlayerName = playerName;
    public readonly DateTime TimeOfDeath = timeOfDeath;
    public readonly Alliance Alliance = alliance;
}