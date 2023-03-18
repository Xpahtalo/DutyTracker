using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.GeneratedSheets;

namespace DutyTracker.Duty_Events;

public class Duty
{
    public TerritoryType TerritoryType { get; }
    public DateTime      StartTime     { get; }
    public DateTime      EndTime       { get; set; }
    public TimeSpan      Duration      => EndTime == DateTime.MinValue ? DateTime.Now - StartTime : EndTime - StartTime;
    public List<Run>     RunList       { get; }

    public Duty(TerritoryType territoryType)
    {
        TerritoryType = territoryType;
        StartTime     = DateTime.Now;
        EndTime       = DateTime.MinValue;
        RunList       = new List<Run>();
    }

    public int TotalDeaths => RunList.Sum(run => run.DeathList.Count);
    public int TotalWipes  => RunList.Count - 1;
}
