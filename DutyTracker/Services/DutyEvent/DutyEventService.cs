using System;
using DutyTracker.Extensions;
using Lumina.Excel.GeneratedSheets;

namespace DutyTracker.Services.DutyEvent;

public sealed class DutyEventService : IDisposable
{
    private bool DutyStarted;

    public event EventHandler<DutyStartedEventArgs>? OnDutyStartedEvent;
    public event EventHandler? OnDutyWipedEvent;
    public event EventHandler? OnDutyRecommencedEvent;
    public event EventHandler<DutyEndedEventArgs>? OnDutyEndedEvent;

    public DutyEventService()
    {
        DutyTracker.DutyState.DutyStarted += OnDutyStarted;
        DutyTracker.DutyState.DutyWiped += OnDutyWiped;
        DutyTracker.DutyState.DutyRecommenced += OnDutyRecommenced;
        DutyTracker.DutyState.DutyCompleted += OnDutyEnded;
        DutyTracker.ClientState.TerritoryChanged += OnTerritoryChanged;
    }

    public void Dispose()
    {
        DutyTracker.DutyState.DutyStarted -= OnDutyStarted;
        DutyTracker.DutyState.DutyWiped -= OnDutyWiped;
        DutyTracker.DutyState.DutyRecommenced -= OnDutyRecommenced;
        DutyTracker.DutyState.DutyCompleted -= OnDutyEnded;
        DutyTracker.ClientState.TerritoryChanged -= OnTerritoryChanged;
    }

    private void OnDutyStarted(object? o, ushort territoryType)
    {
        DutyTracker.Log.Information($"Duty Detected. TerritoryType: {territoryType}");
        var territory = Sheets.TerritorySheet.GetRow(territoryType);
        if (territory is null)
        {
            DutyTracker.Log.Warning("Could not load territory sheet.");
            return;
        }

        DutyTracker.Log.Information($"IntendedUse: {territory.TerritoryIntendedUse}, Name: {territory.Name ?? "No Name"}, PlaceName: {territory.PlaceName.Value?.Name ?? "No Name"}");
        if (!territory.GetIntendedUseEnum().ShouldTrack())
            return;

        DutyStarted = true;
        SafeInvokeDutyStarted(territory);
    }

    private void OnDutyWiped(object? o, ushort territory)
    {
        DutyTracker.Log.Verbose("Duty Wipe");
        SafeInvokeDutyWiped();
    }

    private void OnDutyRecommenced(object? o, ushort territory)
    {
        DutyTracker.Log.Verbose("Duty Recommenced");
        SafeInvokeDutyRecommenced();
    }

    private void OnDutyEnded(object? o, ushort territory)
    {
        if (DutyStarted)
        {
            DutyTracker.Log.Debug("Detected end of duty via DutyState.DutyCompleted");
            SafeInvokeDutyEnded(true);
        }
    }

    // This gets called before DutyState.DutyCompleted, so we can intercept in case the duty is abandoned instead of
    // completed.
    private void OnTerritoryChanged(ushort territoryType)
    {
        if (DutyStarted && DutyTracker.DutyState.IsDutyStarted == false)
        {
            DutyTracker.Log.Debug("Detected end of duty via ClientState.TerritoryChanged");
            SafeInvokeDutyEnded(false);
        }
    }

    // Because events are being invoked while we're still in the client's native code, unhandled exceptions will cause
    // an immediate crash to desktop. Wrapping them like this masks the problem, but I think users would prefer the
    // plugin to be broken than their game to crash.
    private void SafeInvokeDutyStarted(TerritoryType territoryType)
    {
        try
        {
            OnDutyStartedEvent?.Invoke(this, new DutyStartedEventArgs(territoryType));
        }
        catch (Exception e)
        {
            DutyTracker.Log.Error(e, "Unhandled exception when invoking DutyEventService.DutyStarted");
        }
    }

    private void SafeInvokeDutyWiped()
    {
        try
        {
            OnDutyWipedEvent?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            DutyTracker.Log.Error(e, "Unhandled exception when invoking DutyEventService.DutyWiped");
        }
    }

    private void SafeInvokeDutyRecommenced()
    {
        try
        {
            OnDutyRecommencedEvent?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            DutyTracker.Log.Error(e, "Unhandled exception when invoking DutyEventService.DutyRecommenced");
        }
    }

    private void SafeInvokeDutyEnded(bool completed)
    {
        try
        {
            DutyTracker.Log.Verbose($"Duty Ended. Completed: {completed}");
            DutyStarted = false;
            OnDutyEndedEvent?.Invoke(this, new DutyEndedEventArgs(completed));
        }
        catch (Exception e)
        {
            DutyTracker.Log.Error(e, "Unhandled exception when invoking DutyEventService.DutyEnded");
        }
    }
}