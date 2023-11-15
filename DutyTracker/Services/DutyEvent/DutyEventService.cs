using System;
using Dalamud.Plugin.Services;
using DutyTracker.Extensions;
using Lumina.Excel.GeneratedSheets;
using XpahtaLib.DalamudUtilities.Extensions;
using XpahtaLib.DalamudUtilities.UsefulEnums;

namespace DutyTracker.Services.DutyEvent;

public sealed class DutyEventService : IDisposable
{
    private bool _dutyStarted;

    private readonly IDutyState   _dutyState;
    private readonly IDataManager _dataManager;
    private readonly IClientState _clientState;

    public event EventHandler<DutyStartedEventArgs>? DutyStarted;
    public event EventHandler?                       DutyWiped;
    public event EventHandler?                       DutyRecommenced;
    public event EventHandler<DutyEndedEventArgs>?   DutyEnded;


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
        if (!_dutyState.IsDutyStarted)
            return;
        var territory = _dataManager.Excel.GetSheet<TerritoryType>()?.GetRow(_clientState.TerritoryType);
        if (territory is null)
            return;
        SafeInvokeDutyStarted(territory);
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

        Service.PluginLog.Information($"IntendedUse: {territory.TerritoryIntendedUse}, Name: {territory.Name ?? "No Name"}, PlaceName: {territory.PlaceName.Value?.Name ?? "No Name"}");

        if (!territory.GetIntendedUseEnum().ShouldTrack())
            return;

        _dutyStarted = true;
        SafeInvokeDutyStarted(territory);
    }

    private void OnDutyWiped(object? o, ushort territory)
    {
        Service.PluginLog.Verbose("Duty Wipe");
        SafeInvokeDutyWiped();
    }

    private void OnDutyRecommenced(object? o, ushort territory)
    {
        Service.PluginLog.Verbose("Duty Recommenced");
        SafeInvokeDutyRecommenced();
    }

    private void OnDutyEnded(object? o, ushort territory)
    {
        if (_dutyStarted) {
            Service.PluginLog.Debug("Detected end of duty via DutyState.DutyCompleted");
            SafeInvokeDutyEnded(true);
        }
    }

    // This gets called before DutyState.DutyCompleted, so we can intercept in case the duty is abandoned instead of
    // completed.
    private void OnTerritoryChanged(ushort territoryType)
    {
        if (_dutyStarted && _dutyState.IsDutyStarted == false) {
            Service.PluginLog.Debug("Detected end of duty via ClientState.TerritoryChanged");
            SafeInvokeDutyEnded(false);
        }
    }

    // Because events are being invoked while we're still in the client's native code, unhandled exceptions will cause
    // an immediate crash to desktop. Wrapping them like this masks the problem, but I think users would prefer the
    // plugin to be broken than their game to crash.
    private void SafeInvokeDutyStarted(TerritoryType territoryType)
    {
        try {
            DutyStarted?.Invoke(this, new DutyStartedEventArgs(territoryType));
        } catch (Exception e) {
            Service.PluginLog.Error(e, "Unhandled exception when invoking DutyEventService.DutyStarted");
        }
    }

    private void SafeInvokeDutyWiped()
    {
        try {
            DutyWiped?.Invoke(this, EventArgs.Empty);
        } catch (Exception e) {
            Service.PluginLog.Error(e, "Unhandled exception when invoking DutyEventService.DutyWiped");
        }
    }

    private void SafeInvokeDutyRecommenced()
    {
        try {
            DutyRecommenced?.Invoke(this, EventArgs.Empty);
        } catch (Exception e) {
            Service.PluginLog.Error(e, "Unhandled exception when invoking DutyEventService.DutyRecommenced");
        }
    }

    private void SafeInvokeDutyEnded(bool completed)
    {
        try {
            Service.PluginLog.Verbose($"Duty Ended. Completed: {completed}");
            _dutyStarted = false;
            DutyEnded?.Invoke(this, new DutyEndedEventArgs(completed));
        } catch (Exception e) {
            Service.PluginLog.Error(e, "Unhandled exception when invoking DutyEventService.DutyEnded");
        }
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
