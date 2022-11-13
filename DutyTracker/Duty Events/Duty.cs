using System;
using System.Collections.Generic;
using Dalamud.Logging;

namespace DutyTracker.Duty_Events;

/// <summary>
/// Describes an entire Duty lifecycle
/// </summary>
public class Duty
{
    /// <summary>
    /// List of all deaths that occured during this instance.
    /// </summary>
    public List<DeathEvent> DeathEvents { get; private set; }

    /// <summary>
    /// List of all wipes that occured during this instance.
    /// </summary>
    public List<WipeEvent> WipeEvents { get; private set; }

    /// <summary>
    /// The timestamp of the initial start of the instance.
    /// </summary>
    public DateTime StartOfDuty { get; set; }

    /// <summary>
    /// The timestamp of the start of the most recent attempt after a wipe. Matches <see cref="StartOfDuty"/> if there
    /// have been no wipes.
    /// </summary>
    public DateTime StartOfCurrentRun { get; set; }

    /// <summary>
    /// The timestamp of the end of the duty.
    /// </summary>
    public DateTime EndOfDuty { get; set; }


    /// <summary>
    /// Create a new <see cref="Duty"/> starting at the time of creation.
    /// </summary>
    public Duty()
    {
        DeathEvents = new List<DeathEvent>();
        WipeEvents  = new List<WipeEvent>();
        var start = DateTime.Now;
        StartOfDuty       = start;
        StartOfCurrentRun = start;
        EndOfDuty         = DateTime.MinValue;
        PluginLog.Verbose($"Starting a new Duty.");
        PluginLog.Verbose($"    Start Time: {StartOfDuty}");
    }

    /// <summary>
    /// Adds a new <see cref="DeathEvent"/> to the list of deaths.
    /// </summary>
    /// <param name="deathEvent">The <see cref="DeathEvent"/> to be added to the list.</param>
    public void AddDeathEvent(DeathEvent deathEvent)
    {
        DeathEvents.Add(deathEvent);
        PluginLog.Verbose($"Adding a new death event");
        PluginLog.Verbose($"    Player Id: {deathEvent.PlayerId}");
        PluginLog.Verbose($"    Player Name: {deathEvent.PlayerName}");
        PluginLog.Verbose($"    Time of Death: {deathEvent.TimeOfDeath}");
    }

    /// <summary>
    /// Creates a new <see cref="WipeEvent"/> and adds it to the list.
    /// </summary>
    public void AddWipeEvent()
    {
        var end = DateTime.Now;
        var wipeEvent = new WipeEvent
                        {
                            Duration   = end - StartOfCurrentRun,
                            TimeOfWipe = end,
                        };

        WipeEvents.Add(wipeEvent);
        PluginLog.Verbose($"Adding a new wipe event.");
        PluginLog.Verbose($"    Wipe Time: {end}");
    }

    /// <summary>
    /// Starts a new run.
    /// </summary>
    public void StartNewRun()
    {
        StartOfCurrentRun = DateTime.Now;
        PluginLog.Verbose($"Starting a new run");
        PluginLog.Verbose($"    Start of Current Run: {StartOfCurrentRun}");
    }

    /// <summary>
    /// Ends the current duty.
    /// </summary>
    public void EndDuty()
    {
        EndOfDuty = DateTime.Now;
        PluginLog.Verbose($"Ending the current duty");
        PluginLog.Verbose($"    End of Duty: {EndOfDuty}");
    }
}
