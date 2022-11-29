﻿using System;
using System.Linq;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using DutyTracker.Extensions;

namespace DutyTracker.Duty_Events;

public class DutyManager
{
    /// <summary>
    /// The current <see cref="Duty"/> being tracked.
    /// </summary>
    public Duty Duty { get; set; }

    /// <summary>
    /// Is a duty currently active.
    /// </summary>
    public bool DutyActive { get; set; }

    public bool AnyDutiesStarted { get; set; }

    /// <summary>
    /// How elapsed time so far of the current duty, or how long it lasted if it is now over.
    /// </summary>
    public TimeSpan TotalDutyDuration => DutyActive ? DateTime.Now - Duty.StartOfDuty : Duty.EndOfDuty - Duty.StartOfDuty;

    /// <summary>
    /// The elapsed time so far of the current run, or how long the last run was if the duty is now over.
    /// </summary>
    public TimeSpan CurrentRunDuration => DutyActive ? DateTime.Now - Duty.StartOfCurrentRun : Duty.EndOfDuty - Duty.StartOfCurrentRun;
    
    public TimeSpan FinalRunDuration => (Duty.WipeEvents.Count == 0) ? Duty.EndOfDuty - Duty.StartOfDuty : Duty.WipeEvents[^1].Duration;


    private readonly Configuration configuration;
    private readonly ChatGui       chatGui;

    public DutyManager(
        Configuration                    configuration,
        [RequiredVersion("1.0")] ChatGui chatGui)
    {
        this.configuration = configuration;
        this.chatGui       = chatGui;
        Duty               = new Duty();
        DutyActive         = false;
        AnyDutiesStarted   = false;
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

        DutyActive       = true;
        AnyDutiesStarted = true;
    }

    /// <summary>
    /// Ends the current duty.
    /// </summary>
    public void EndDuty()
    {
        Duty.EndDuty();
        DutyActive  = false;

        chatGui.Print(InfoMessage("Time in Duty: ", $"{TotalDutyDuration.MinutesAndSeconds()}"));
        if (Duty.WipeEvents.Count > 0 || !configuration.SuppressEmptyValues)
        {
            chatGui.Print(InfoMessage("Final Run Duration: ", $"{CurrentRunDuration.MinutesAndSeconds()}"));
            chatGui.Print(InfoMessage("Wipes: ",              $"{Duty.WipeEvents.Count}"));
        }

        if (Duty.DeathEvents.Count > 0 || !configuration.SuppressEmptyValues)
            chatGui.Print(InfoMessage("Party Deaths: ", $"{Duty.DeathEvents.Count}"));
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

    private SeString InfoMessage(string label, string info)
    {
        var seStringBuilder = new SeStringBuilder();

        if (configuration.IncludeDutyTrackerLabel)
        {
            seStringBuilder.AddUiForeground("[DutyTracker] ", 35).AddUiForegroundOff();
        }

        seStringBuilder.AddUiForeground(label, 62).AddUiForegroundOff()
                       .AddUiForeground(info, 45).AddUiForegroundOff();

        return seStringBuilder.Build();
    }
}
