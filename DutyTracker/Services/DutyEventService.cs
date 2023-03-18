using System;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.DutyState;
using Dalamud.Logging;
using DutyTracker.Enums;
using Lumina.Excel.GeneratedSheets;

namespace DutyTracker.Services;


public class DutyStartedEventArgs : EventArgs
{
    public TerritoryType        TerritoryType { get; }
    public TerritoryIntendedUse IntendedUse   => (TerritoryIntendedUse)TerritoryType.TerritoryIntendedUse;
    
    public DutyStartedEventArgs(TerritoryType territoryType) { TerritoryType = territoryType; }
}

public class DutyEndedEventArgs : EventArgs
{
    public bool Completed { get; }
    
    public DutyEndedEventArgs(bool completed) { Completed = completed; }
}

public sealed class DutyEventService : IDisposable
{
    private bool _dutyStarted;
    
    private readonly DutyState   _dutyState;
    private readonly DataManager _dataManager;
    private readonly ClientState _clientState;

    public delegate void DutyStartedDelegate(DutyStartedEventArgs eventArgs);
    public delegate void DutyWipedDelegate();
    public delegate void DutyRecommencedDelegate();
    public delegate void DutyEndedDelegate(DutyEndedEventArgs eventArgs);

    public event DutyStartedDelegate?     DutyStarted;
    public event DutyWipedDelegate?       DutyWiped;
    public event DutyRecommencedDelegate? DutyRecommenced;
    public event DutyEndedDelegate?       DutyEnded;
    

    public DutyEventService()
    {
        _dutyState   = Service.DutyState;
        _dataManager = Service.DataManager;
        _clientState = Service.ClientState;

        _dutyState.DutyStarted        += OnDutyStarted;
        _dutyState.DutyWiped          += OnDutyWiped;
        _dutyState.DutyRecommenced    += OnDutyRecommenced;
        _dutyState.DutyCompleted      += OnDutyEnded;
        _clientState.TerritoryChanged += OnTerritoryChanged;
    }

    // This gets called before DutyState.DutyCompleted, so we can intercept in case the duty is abandoned instead of completed. 
    private void OnTerritoryChanged(object? o, ushort territoryType)
    {
        if (_dutyStarted && _dutyState.IsDutyStarted == false)
            EndDuty(false);
    }

    private void OnDutyStarted(object? o, ushort territoryType)
    {
        var territory = _dataManager.Excel.GetSheet<TerritoryType>()?.GetRow(territoryType);
        if (territory is null)
            return;
        if (!((TerritoryIntendedUse)territory.TerritoryIntendedUse).ShouldTrack())
            return;

        _dutyStarted = true;
        PluginLog.Verbose($"Duty Started: {territory.Name ?? "No Name"}");
        DutyStarted?.Invoke(new DutyStartedEventArgs(territory!));
    }

    private void OnDutyWiped(object? o, ushort territory)
    {
        PluginLog.Verbose("Duty Wipe");
        DutyWiped?.Invoke();
    }

    private void OnDutyRecommenced(object? o, ushort territory)
    {
        PluginLog.Verbose("Duty Recommenced");
        DutyRecommenced?.Invoke();
    }

    private void OnDutyEnded(object? o, ushort territory)
    {
        if (_dutyStarted)
            EndDuty(true);
    }

    private void EndDuty(bool completed)
    {
        PluginLog.Verbose($"Duty Ended. Completed: {completed}");
        _dutyStarted = false;
        DutyEnded?.Invoke(new DutyEndedEventArgs(completed));
    }

    public void Dispose()
    {
        _dutyState.DutyStarted        -= OnDutyStarted;
        _dutyState.DutyWiped          -= OnDutyWiped;
        _dutyState.DutyRecommenced    -= OnDutyRecommenced;
        _dutyState.DutyCompleted      -= OnDutyEnded;
        _clientState.TerritoryChanged -= OnTerritoryChanged;
    }
}
