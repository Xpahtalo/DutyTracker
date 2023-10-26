using System;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
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
    
    private readonly IDutyState   _dutyState;
    private readonly IDataManager _dataManager;
    private readonly IClientState _clientState;

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

    
#if DEBUG
    public void Debug()
    {
        if (_dutyState.IsDutyStarted)
            DutyStarted?.Invoke(new DutyStartedEventArgs(_dataManager.Excel.GetSheet<TerritoryType>()?.GetRow(_clientState.TerritoryType)!));
    }
#endif
    
    private void OnDutyStarted(object? o, ushort territoryType)
    {
        Service.PluginLog.Information($"Duty Detected. TerritoryType: {territoryType}");
        var territory = _dataManager.Excel.GetSheet<TerritoryType>()?.GetRow(territoryType);
        if (territory is null) {
            Service.PluginLog.Warning("Could not load territory sheet.");
            return;
        }
        Service.PluginLog.Information($"IntendedUse: {territory.TerritoryIntendedUse}, Name: {territory.Name ?? "No Name"}, PlaceName: {territory.PlaceName.Value?.Name?? "No Name"}" );
        
        
        if (!((TerritoryIntendedUse)territory.TerritoryIntendedUse).ShouldTrack())
            return;

        _dutyStarted = true;
        DutyStarted?.Invoke(new DutyStartedEventArgs(territory!));
    }

    private void OnDutyWiped(object? o, ushort territory)
    {
        Service.PluginLog.Verbose("Duty Wipe");
        DutyWiped?.Invoke();
    }

    private void OnDutyRecommenced(object? o, ushort territory)
    {
        Service.PluginLog.Verbose("Duty Recommenced");
        DutyRecommenced?.Invoke();
    }

    private void OnDutyEnded(object? o, ushort territory)
    {
        if (_dutyStarted) {
            Service.PluginLog.Debug("Detected end of duty via DutyState.DutyCompleted");
            EndDuty(true);
        }
    }
    
    // This gets called before DutyState.DutyCompleted, so we can intercept in case the duty is abandoned instead of completed. 
    private void OnTerritoryChanged(ushort territoryType)
    {
        if (_dutyStarted && _dutyState.IsDutyStarted == false) {
            Service.PluginLog.Debug("Detected end of duty via ClientState.TerritoryChanged");
            EndDuty(false);
        }
    }
    
    private void EndDuty(bool completed)
    {
        Service.PluginLog.Verbose($"Duty Ended. Completed: {completed}");
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
