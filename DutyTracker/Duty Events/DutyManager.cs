using System;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using DutyTracker.Extensions;

namespace DutyTracker.Duty_Events;

public class DutyManager
{
    private Duty     currentDuty;
    private Run      currentRun;

    public bool       DutyActive       { get; private set; }
    public List<Duty> Duties           { get; private set; }
    public bool       AnyDutiesStarted { get; private set; }


    private readonly Configuration configuration;

    public DutyManager(Configuration configuration)
    {
        this.configuration = configuration;
        DutyActive         = false;
        Duties             = new List<Duty>();
        currentDuty        = new Duty();
        currentRun         = new Run();
    }

    public void StartDuty()
    {
        DutyActive       = true;
        AnyDutiesStarted = true;
        currentDuty = new Duty
                      {
                          TerritoryType = Service.ClientState.TerritoryType,
                      };
        Duties.Add(currentDuty);
        StartNewRun();
    }

    public void EndDuty()
    {
        DutyActive          = false;
        currentDuty.EndTime = DateTime.Now;
        
        EndRun();

        var dutyDuration = currentDuty.EndTime - currentDuty.StartTime;

        Service.ChatGui.Print(InfoMessage("Time in Duty: ", $"{dutyDuration.MinutesAndSeconds()}"));
        if (currentDuty.RunList.Count > 1 || !configuration.SuppressEmptyValues)
        {
            var finalRun         = currentDuty.RunList[^1];
            var finalRunDuration = finalRun.EndTime - finalRun.StartTime;
            
            Service.ChatGui.Print(InfoMessage("Final Run Duration: ", $"{finalRunDuration.MinutesAndSeconds()}"));
            Service.ChatGui.Print(InfoMessage("Wipes: ",              $"{currentDuty.TotalWipes}"));
        }

        var totalDeaths = currentDuty.TotalDeaths;
        
        if (totalDeaths > 0 || !configuration.SuppressEmptyValues)
            Service.ChatGui.Print(InfoMessage("Party Deaths: ", $"{totalDeaths}"));
    }

    public void AddDeath(Death death)
    {
        currentRun.DeathList.Add(death);
    }

    public void EndRun()
    {
        currentRun.EndTime = DateTime.Now;
    }

    public void StartNewRun()
    {
        currentRun = new Run();
        currentDuty.RunList.Add(currentRun);
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
