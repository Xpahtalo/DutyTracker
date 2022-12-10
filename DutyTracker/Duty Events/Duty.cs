using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;

namespace DutyTracker.Duty_Events;

public class Duty
{
    public ushort    TerritoryType { get; set; }
    public DateTime  StartTime     { get; set; } = DateTime.Now;
    public DateTime  EndTime       { get; set; } = DateTime.MinValue;
    public List<Run> RunList       { get; set; } = new List<Run>();

    public int TotalDeaths => RunList.Sum(run => run.DeathList.Count);
    public int TotalWipes  => RunList.Count - 1;
}
