using System;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using DutyTracker.Enums;
using DutyTracker.Extensions;
using DutyTracker.Services;
using Lumina.Excel.GeneratedSheets;

namespace DutyTracker.Duty_Events;

public class DutyManager : IDisposable
{
    private readonly DutyEventService     _dutyEventService;
    private readonly PlayerCharacterState _playerCharacterState;

    private Duty? _currentDuty;
    private Run?  _currentRun;

    public bool       DutyActive       { get; private set; }
    public List<Duty> DutyList         { get; private set; }
    public bool       AnyDutiesStarted { get; private set; }

    public Duty? GetMostRecentDuty()
    {
        if (_currentDuty is not null)
            return _currentDuty;
        if (DutyList.Count > 0)
            return DutyList[^1];
        return null;
    }

    public Run? GetMostRecentRun()
    {
        if (_currentDuty is not null)
            return _currentRun;
        if (GetMostRecentDuty()?.RunList.Count > 0)
            return GetMostRecentDuty()?.RunList[^1];
        return null;
    }

    private readonly Configuration _configuration;

    public DutyManager(Configuration configuration)
    {
        _dutyEventService     = Service.DutyEventService;
        _playerCharacterState = Service.PlayerCharacterState;
        _configuration        = configuration;
        DutyActive            = false;
        DutyList              = new List<Duty>();
        _currentDuty          = null;
        _currentRun           = null;

        _playerCharacterState.OnPlayerDeath += AddDeath;
        _dutyEventService.DutyStarted       += StartDuty;
        _dutyEventService.DutyWiped         += EndRun;
        _dutyEventService.DutyRecommenced   += StartNewRun;
        _dutyEventService.DutyEnded         += EndDuty;
    }
    
    public void Dispose()
    {
        _playerCharacterState.OnPlayerDeath -= AddDeath;
        _dutyEventService.DutyStarted       -= StartDuty;
        _dutyEventService.DutyWiped         -= EndRun;
        _dutyEventService.DutyRecommenced   -= StartNewRun;
        _dutyEventService.DutyEnded         -= EndDuty;
    }

    private void StartDuty(TerritoryType territoryType)
    {
        DutyActive       = true;
        AnyDutiesStarted = true;
        _currentDuty     = new Duty(territoryType);

        StartNewRun();
    }

    private void EndDuty()
    {
        DutyActive            = false;
        _currentDuty!.EndTime = DateTime.Now;

        EndRun();

        var dutyDuration = _currentDuty.EndTime - _currentDuty.StartTime;

        Service.ChatGui.Print(InfoMessage("Time in Duty: ", $"{dutyDuration.MinutesAndSeconds()}"));
        if (_currentDuty.RunList.Count > 1 || !_configuration.SuppressEmptyValues) {
            var finalRun         = _currentDuty.RunList[^1];
            var finalRunDuration = finalRun.EndTime - finalRun.StartTime;

            Service.ChatGui.Print(InfoMessage("Final Run Duration: ", $"{finalRunDuration.MinutesAndSeconds()}"));
            Service.ChatGui.Print(InfoMessage("Wipes: ",              $"{_currentDuty.TotalWipes}"));
        }

        var totalDeaths = _currentDuty.TotalDeaths;

        if (totalDeaths > 0 || !_configuration.SuppressEmptyValues)
            Service.ChatGui.Print(InfoMessage("Party Deaths: ", $"{totalDeaths}"));
        DutyList.Add(_currentDuty);
    }

    private void AddDeath(string playerName, uint objectId, Alliance alliance) { _currentRun?.DeathList.Add(new Death(objectId, playerName, DateTime.Now, alliance)); }

    private void EndRun() { _currentRun!.EndTime = DateTime.Now; }

    private void StartNewRun()
    {
        _currentRun = new Run();
        _currentDuty!.RunList.Add(_currentRun);
    }

    private SeString InfoMessage(string label, string info)
    {
        var seStringBuilder = new SeStringBuilder();

        if (_configuration.IncludeDutyTrackerLabel) {
            seStringBuilder.AddUiForeground("[DutyTracker] ", 35).AddUiForegroundOff();
        }

        seStringBuilder.AddUiForeground(label, 62).AddUiForegroundOff()
                       .AddUiForeground(info, 45).AddUiForegroundOff();

        return seStringBuilder.Build();
    }


}
