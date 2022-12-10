using System;
using System.Collections.Generic;

namespace DutyTracker.Duty_Events;

public class Run
{
    public DateTime    StartTime { get; set; } = DateTime.Now;
    public DateTime    EndTime   { get; set; } = DateTime.MinValue;
    public List<Death> DeathList { get; set; } = new List<Death>();
}
