using System;
using System.Text;

namespace DutyTracker.Extensions;

internal static class TimeSpanExtensions
{
    public static string MinutesAndSeconds(this TimeSpan timeSpan)
    {
        return $"{Math.Floor(timeSpan.TotalMinutes)}:{timeSpan.Seconds}";
    }

    public static string HoursMinutesAndSeconds(this TimeSpan timeSpan)
    {
        var stringBuilder = new StringBuilder();
        if (timeSpan.TotalHours > 1)
        {
            stringBuilder.Append(timeSpan.Hours);
            stringBuilder.Append(':');
        }

        stringBuilder.Append(timeSpan.Minutes);
        stringBuilder.Append(':');
        stringBuilder.Append(timeSpan.Seconds);
        
        return stringBuilder.ToString();
    }
}
