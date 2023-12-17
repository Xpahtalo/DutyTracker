using System;
using Lumina.Excel.GeneratedSheets;
using XpahtaLib.DalamudUtilities.Extensions;
using XpahtaLib.DalamudUtilities.UsefulEnums;

namespace DutyTracker.Services.DutyEvent;

public class DutyStartedEventArgs : EventArgs
{
    public TerritoryType            TerritoryType { get; }
    public TerritoryIntendedUseEnum IntendedUse   => TerritoryType.GetIntendedUseEnum();

    public DutyStartedEventArgs(TerritoryType territoryType) { TerritoryType = territoryType; }
}
