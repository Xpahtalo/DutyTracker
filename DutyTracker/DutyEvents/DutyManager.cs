using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using DutyTracker.Enums;
using DutyTracker.Extensions;
using DutyTracker.Services.DutyEvent;
using DutyTracker.Services.PlayerCharacter;

namespace DutyTracker.DutyEvents;

public class DutyManager : IDisposable
{
    private readonly DutyTracker DutyTracker;

    private Run? CurrentRun;
    private Duty? CurrentDuty;

    public bool DutyActive;
    public bool AnyDutiesStarted;
    public readonly List<Duty> DutyList;

    public DutyManager(DutyTracker dutyTracker)
    {
        DutyTracker = dutyTracker;
        DutyActive = false;
        DutyList = [];
        CurrentRun = null;
        CurrentDuty = null;

        DutyTracker.PlayerCharacterState.OnPlayerDeath += AddDeath;
        DutyTracker.DutyEventService.OnDutyStartedEvent += OnDutyStarted;
        DutyTracker.DutyEventService.OnDutyWipedEvent += OnDutyWiped;
        DutyTracker.DutyEventService.OnDutyRecommencedEvent += OnDutyRecommenced;
        DutyTracker.DutyEventService.OnDutyEndedEvent += OnDutyEnded;
    }

    public void Dispose()
    {
        DutyTracker.PlayerCharacterState.OnPlayerDeath -= AddDeath;
        DutyTracker.DutyEventService.OnDutyStartedEvent -= OnDutyStarted;
        DutyTracker.DutyEventService.OnDutyWipedEvent -= OnDutyWiped;
        DutyTracker.DutyEventService.OnDutyRecommencedEvent -= OnDutyRecommenced;
        DutyTracker.DutyEventService.OnDutyEndedEvent -= OnDutyEnded;
    }

    public Duty? GetMostRecentDuty()
    {
        if (CurrentDuty is not null)
            return CurrentDuty;

        return DutyList.Count > 0 ? DutyList[^1] : null;
    }

    public Run? GetMostRecentRun()
    {
        if (CurrentRun is not null)
            return CurrentRun;

        var runList = GetMostRecentDuty()?.RunList;
        return runList?.Count > 0 ? runList[^1] : null;
    }

    // Really crappy way to pass in the args until I have time to redo this plugin.
    private void OnDutyStarted(object? o, DutyStartedEventArgs args) => StartDuty(args);
    private void OnDutyWiped(object? o, EventArgs args) => EndRun();
    private void OnDutyRecommenced(object? o, EventArgs args) => StartNewRun();
    private void OnDutyEnded(object? o, DutyEndedEventArgs args) => EndDuty(args);


    private void StartDuty(DutyStartedEventArgs eventArgs)
    {
        DutyActive = true;
        AnyDutiesStarted = true;
        CurrentDuty = new Duty(eventArgs.TerritoryType, eventArgs.IntendedUse.GetAllianceType());

        StartNewRun();
    }

    private void EndDuty(DutyEndedEventArgs eventArgs)
    {
        DutyActive = false;
        if (CurrentDuty is null)
            return;

        CurrentDuty.EndTime = DateTime.Now;

        if (eventArgs.Completed)
        {
            EndRun();
        }
        else
        {
            if (CurrentRun is not null)
            {
                CurrentDuty.RunList.Remove(CurrentRun);
                CurrentRun = null;
            }
        }

        var dutyDuration = CurrentDuty.EndTime - CurrentDuty.StartTime;
        DutyTracker.ChatGui.Print(InfoMessage("Time in Duty: ", $"{dutyDuration.MinutesAndSeconds()}"));

        if (CurrentDuty.RunList.Count > 1 || !DutyTracker.Configuration.SuppressEmptyValues)
        {
            var finalRun = CurrentDuty.RunList[^1];
            var finalRunDuration = finalRun.EndTime - finalRun.StartTime;

            DutyTracker.ChatGui.Print(InfoMessage("Final Run Duration: ", $"{finalRunDuration.MinutesAndSeconds()}"));
            DutyTracker.ChatGui.Print(InfoMessage("Wipes: ", $"{CurrentDuty.TotalWipes}"));
        }

        switch (CurrentDuty.AllianceType)
        {
            case AllianceType.ThreeParty:
                foreach (Alliance alliance in Enum.GetValues(typeof(Alliance)))
                {
                    if (alliance is Alliance.D or Alliance.E or Alliance.F or Alliance.None)
                        continue;

                    var count = CurrentDuty.AllDeaths.Count(x => x.Alliance == alliance);
                    DutyTracker.ChatGui.Print(InfoMessage($"{alliance} deaths: ", count.ToString(), count == 0));
                }

                break;
            case AllianceType.SixParty:
                foreach (Alliance alliance in Enum.GetValues(typeof(Alliance)))
                {
                    if (alliance is Alliance.None)
                        continue;

                    var count = CurrentDuty.AllDeaths.Count(x => x.Alliance == alliance);
                    DutyTracker.ChatGui.Print(InfoMessage($"{alliance} deaths: ", count.ToString(), count == 0));
                }

                break;
            case AllianceType.None:
            default:
                if (CurrentDuty.AllDeaths.Count > 0 || !DutyTracker.Configuration.SuppressEmptyValues)
                    DutyTracker.ChatGui.Print(InfoMessage("Party Deaths: ", $"{CurrentDuty.AllDeaths.Count}"));
                break;
        }

        DutyList.Add(CurrentDuty);
    }

    private void AddDeath(object? o, PlayerDeathEventArgs eventArgs)
    {
        CurrentRun?.DeathList.Add(new Death(eventArgs.PlayerName, DateTime.Now, eventArgs.Alliance));
    }

    private void EndRun()
    {
        if (CurrentRun is not null)
            CurrentRun.EndTime = DateTime.Now;
    }

    private void StartNewRun()
    {
        if (CurrentDuty is null)
        {
            var territory = Sheets.TerritorySheet.GetRow(DutyTracker.ClientState.TerritoryType);
            if (territory is null)
                return;

            StartDuty(new DutyStartedEventArgs(territory));
        }

        CurrentRun = new Run();
        CurrentDuty?.RunList.Add(CurrentRun);
    }

    private SeString InfoMessage(string label, string info, bool highlightInfo = false)
    {
        var seStringBuilder = new SeStringBuilder();
        if (DutyTracker.Configuration.IncludeDutyTrackerLabel)
            seStringBuilder.AddUiForeground("[DutyTracker] ", 35);

        seStringBuilder.AddUiForeground(label, 62);
        if (highlightInfo)
            seStringBuilder.AddUiGlow(info, 45);
        else
            seStringBuilder.AddUiForeground(info, 45);

        return seStringBuilder.Build();
    }
}