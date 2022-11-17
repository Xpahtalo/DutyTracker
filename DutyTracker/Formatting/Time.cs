using System;

namespace DutyTracker.Formatting;

public static class TimeFormat
{
    public static string MinutesAndSeconds(TimeSpan timeSpan)
    {
        return $"{timeSpan.TotalMinutes}:{timeSpan.Seconds}";
    }
}
