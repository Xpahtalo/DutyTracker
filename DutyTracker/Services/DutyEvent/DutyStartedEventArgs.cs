using System;
using Lumina.Excel.GeneratedSheets;
using XpahtaLib.DalamudUtilities.UsefulEnums;

namespace DutyTracker.Services.DutyEvent;

public class DutyStartedEventArgs : EventArgs
{
    public TerritoryType            TerritoryType { get; }
    public TerritoryIntendedUseEnum IntendedUse   => (TerritoryIntendedUseEnum)TerritoryType.TerritoryIntendedUse;

    public DutyStartedEventArgs(TerritoryType territoryType) { TerritoryType = territoryType; }
}
