using System;
using DutyTracker.Enums;

namespace DutyTracker.Services.PlayerCharacter;

public class PlayerDeathEventArgs(string name, Alliance alliance) : EventArgs
{
    public readonly string PlayerName = name;
    public readonly Alliance Alliance = alliance;
}