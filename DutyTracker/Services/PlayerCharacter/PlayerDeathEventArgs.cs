using System;
using DutyTracker.Enums;

namespace DutyTracker.Services.PlayerCharacter;

public class PlayerDeathEventArgs : EventArgs
{
    public string   PlayerName { get; }
    public uint     ObjectId   { get; }
    public Alliance Alliance   { get; }

    public PlayerDeathEventArgs(string name, uint objectId, Alliance alliance)
    {
        PlayerName = name;
        ObjectId   = objectId;
        Alliance   = alliance;
    }
}
