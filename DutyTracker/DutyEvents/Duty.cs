using System;
using System.Collections.Generic;
using System.Linq;
using DutyTracker.Enums;
using Lumina.Excel.GeneratedSheets;

namespace DutyTracker.DutyEvents;

public class Duty(TerritoryType territoryType, AllianceType allianceType)
{
    public TerritoryType TerritoryType { get; } = territoryType;
    public readonly DateTime StartTime = DateTime.Now;
    public DateTime EndTime = DateTime.MinValue;
    public TimeSpan Duration => EndTime == DateTime.MinValue ? DateTime.Now - StartTime : EndTime - StartTime;
    public readonly List<Run> RunList = [];
    public readonly AllianceType AllianceType = allianceType;

    public int TotalDeaths => RunList.Sum(run => run.DeathList.Count);
    public int TotalWipes => int.Max(RunList.Count - 1, 0);

    public List<Death> AllDeaths
    {
        get { return RunList.Aggregate(new List<Death>(), (x, y) => x.Concat(y.DeathList).ToList()); }
    }
}