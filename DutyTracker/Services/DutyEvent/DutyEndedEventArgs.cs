using System;

namespace DutyTracker.Services.DutyEvent;

public class DutyEndedEventArgs : EventArgs
{
    public bool Completed { get; }

    public DutyEndedEventArgs(bool completed) { Completed = completed; }
}
