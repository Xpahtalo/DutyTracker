using System;
using Dalamud.Data;
using Dalamud.Game.DutyState;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;

namespace DutyTracker.Services;

public sealed class DutyEventService : IDisposable
{
    private readonly DutyState   _dutyState;
    private readonly DataManager _dataManager;


    public delegate void DutyStartedDelegate(TerritoryType territoryType);
    public delegate void DutyWipedDelegate();
    public delegate void DutyRecommencedDelegate();
    public delegate void DutyEndedDelegate();


    public event DutyStartedDelegate?     DutyStarted;
    public event DutyWipedDelegate?       DutyWiped;
    public event DutyRecommencedDelegate? DutyRecommenced;
    public event DutyEndedDelegate?       DutyEnded;



    public DutyEventService()
    {
        _dutyState   = Service.DutyState;
        _dataManager = Service.DataManager;

        _dutyState.DutyStarted     += OnDutyStarted;
        _dutyState.DutyWiped       += OnDutyWiped;
        _dutyState.DutyRecommenced += OnDutyRecommenced;
        _dutyState.DutyCompleted   += OnDutyEnded;
    }


    private void OnDutyStarted(object? o, ushort territoryType)
    {
        var territory = _dataManager.Excel.GetSheet<TerritoryType>()?.GetRow(territoryType);
        PluginLog.Verbose($"Duty Started: {territory?.Name ?? "No Name"}");
        if (territory != null) DutyStarted?.Invoke(territory);
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
        PluginLog.Verbose("Duty Ended");
        DutyEnded?.Invoke();
    }

    public void Dispose()
    {
        _dutyState.DutyStarted     -= OnDutyStarted;
        _dutyState.DutyWiped       -= OnDutyWiped;
        _dutyState.DutyRecommenced -= OnDutyRecommenced;
        _dutyState.DutyCompleted   -= OnDutyEnded;
    }
}
