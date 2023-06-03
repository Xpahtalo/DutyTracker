using System;
using System.Text;

namespace DutyTracker.Extensions;

internal static class TimeSpanExtensions
{
    public static string MinutesAndSeconds(this TimeSpan timeSpan)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Math.Floor(timeSpan.TotalMinutes));
        stringBuilder.Append(':');
        if (timeSpan.Seconds < 10)
            stringBuilder.Append('0');
        stringBuilder.Append(timeSpan.Seconds);
        
        return stringBuilder.ToString();
    }

    public static string HoursMinutesAndSeconds(this TimeSpan timeSpan)
    {
        var stringBuilder = new StringBuilder();
        if (timeSpan.TotalHours > 1)
        {
            stringBuilder.Append(Math.Floor(timeSpan.TotalHours));
            stringBuilder.Append(':');
        }

        if (timeSpan.Minutes < 0)
            stringBuilder.Append('0');
        stringBuilder.Append(timeSpan.Minutes);
        stringBuilder.Append(':');
        if (timeSpan.Seconds < 10)
            stringBuilder.Append('0');
        stringBuilder.Append(timeSpan.Seconds);
        
        return stringBuilder.ToString();
    }
}
