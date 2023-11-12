using System;
using DutyTracker.Enums;
using Lumina.Excel.GeneratedSheets;

namespace DutyTracker.Services.DutyEvent;

public class DutyStartedEventArgs : EventArgs
{
    public TerritoryType        TerritoryType { get; }
    public TerritoryIntendedUse IntendedUse   => (TerritoryIntendedUse)TerritoryType.TerritoryIntendedUse;

    public DutyStartedEventArgs(TerritoryType territoryType) { TerritoryType = territoryType; }
}
