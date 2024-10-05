using System;
using System.Collections.Generic;

namespace DutyTracker.DutyEvents;

public class Run
{
    public DateTime StartTime = DateTime.Now;
    public DateTime EndTime = DateTime.MinValue;
    public TimeSpan Duration => EndTime == DateTime.MinValue ? DateTime.Now - StartTime : EndTime - StartTime;
    public readonly List<Death> DeathList = [];
}