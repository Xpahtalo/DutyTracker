using System;

namespace DutyTracker.Services.DutyEvent;

public class DutyEndedEventArgs(bool completed) : EventArgs
{
    public readonly bool Completed = completed;
}