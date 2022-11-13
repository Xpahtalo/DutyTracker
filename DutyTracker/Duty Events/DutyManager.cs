using System;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;

namespace DutyTracker.Duty_Events;

public class DutyManager
{
    [PluginService][RequiredVersion("1.0")] public ChatGui ChatGui { get; set; }
    
    /// <summary>
    /// The current <see cref="Duty"/> being tracked.
    /// </summary>
    public Duty Duty { get; set; }

    /// <summary>
    /// Is a duty currently active.
    /// </summary>
    public bool DutyActive { get; set; }

    /// <summary>
    /// How elapsed time so far of the current duty, or how long it lasted if it is now over.
    /// </summary>
    public TimeSpan TotalDutyTime => DutyActive ? DateTime.Now - Duty.StartOfDuty : Duty.EndOfDuty - Duty.StartOfDuty;

    /// <summary>
    /// The elapsed time so far of the current run, or how long the last run was if the duty is now over.
    /// </summary>
    public TimeSpan CurrentRunTime => DutyActive ? DateTime.Now - Duty.StartOfCurrentRun : Duty.EndOfDuty - Duty.StartOfCurrentRun;

    public DutyManager()
    {
        Duty       = new Duty();
        DutyActive = false;
    }

    /// <summary>
    /// Starts a new instance.
    /// </summary>
    public void StartDuty()
    {
        Duty = new Duty
               {
                   StartOfDuty = DateTime.Now,
               };

        DutyActive = true;
    }
    
    /// <summary>
    /// Ends the current duty.
    /// </summary>
    public void EndDuty()
    {
        Duty.EndDuty();
        DutyActive = false;
        
        ChatGui.Print(InfoMessage("Time in Duty: ", $"{TotalDutyTime:m\\:ss}"));
        ChatGui.Print(InfoMessage("Final Run Duration: ", $"{CurrentRunTime:m\\:ss}"));
        ChatGui.Print(InfoMessage("Deaths: ",       $"{Duty.DeathEvents.Count}"));
        ChatGui.Print(InfoMessage("Wipes: ",        $"{Duty.WipeEvents.Count}"));
    }

    /// <summary>
    /// Adds a new <see cref="DeathEvent"/> to the list of deaths.
    /// </summary>
    /// <param name="deathEvent">The <see cref="DeathEvent"/> to be added to the list.</param>
    public void AddDeathEvent(DeathEvent deathEvent)
    {
        Duty.AddDeathEvent(deathEvent);
    }

    /// <summary>
    /// Triggers a new wipe event in <see cref="Duty"/>.
    /// </summary>
    public void AddWipeEvent()
    {
        Duty.AddWipeEvent();
    }

    /// <summary>
    /// Begins the timer on a new run after a wipe.
    /// </summary>
    public void StartNewRun()
    {
        Duty.StartNewRun();
    }
    
    private static SeString InfoMessage(string label, string info)
    {
        return new SeStringBuilder()
              .AddUiForeground("[DutyTracker] ", 35)
              .AddUiForegroundOff()
               
              .AddUiForeground(label, 62)
              .AddUiForegroundOff()
               
              .AddUiForeground(info, 45)
              .AddUiForegroundOff()
               
              .Build();
    }
}
