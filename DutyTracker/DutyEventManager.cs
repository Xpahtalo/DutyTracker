using System;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using DutyTracker.Duty_Events;

namespace DutyTracker;

// Adapted from NoTankYou (https://github.com/MidoriKami/NoTankYou)
// https://github.com/MidoriKami/NoTankYou/blob/master/NoTankYou/System/DutyEventManager.cs

// ReSharper disable once ClassNeverInstantiated.Global
public unsafe class DutyEventManager : IDisposable
{
    public DateTime StartTime   { get; set; }
    public DateTime EndTime     { get; set; }
    public TimeSpan ElapsedTime => IsBoundByDuty() ? DateTime.Now - StartTime : EndTime - StartTime;

    private delegate byte DutyEventDelegate(void* a1, void* a2, ushort* a3);

    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B D9 49 8B F8 41 0F B7 08", DetourName = nameof(DutyEventFunction))]
    private readonly Hook<DutyEventDelegate>? DutyEventHook = null;

    public           bool        DutyStarted { get; private set; }
    private          bool        completedThisTerritory;
    private readonly DutyManager dutyManager;

    public DutyEventManager(DutyManager dutyManager)
    {
        this.dutyManager = dutyManager;
        SignatureHelper.Initialise(this);

        DutyEventHook?.Enable();

        if (IsBoundByDuty())
        {
            StartDuty();
        }

        Service.Framework.Update             += FrameworkUpdate;
        Service.ClientState.TerritoryChanged += TerritoryChanged;

        PluginLog.Debug("DutyEventManager initialized");
    }

    public void Dispose()
    {
        DutyEventHook?.Dispose();

        Service.Framework.Update             -= FrameworkUpdate;
        Service.ClientState.TerritoryChanged -= TerritoryChanged;
    }

    private void FrameworkUpdate(Framework framework)
    {
        if (!DutyStarted && !completedThisTerritory)
        {
            if (IsBoundByDuty() && Service.Condition[ConditionFlag.InCombat])
            {
                PluginLog.Debug("Start Duty | FrameworkUpdate");
                StartDuty();
            }
        }
    }

    private void TerritoryChanged(object? sender, ushort e)
    {
        if (DutyStarted)
        {
            PluginLog.Debug("Stop Duty | TerritoryChanged");
            StopDuty();
        }

        completedThisTerritory = false;
    }

    private byte DutyEventFunction(void* a1, void* a2, ushort* a3)
    {
        try
        {
            var category = *(a3);
            var type     = *(uint*)(a3 + 4);

            // DirectorUpdate Category
            if (category == 0x6D)
            {
                switch (type)
                {
                    // Duty Commenced
                    case 0x40000001:
                        PluginLog.Debug("Duty Started | DutyEventFunction");
                        StartDuty();
                        break;

                    // Party Wipe
                    case 0x40000005:
                        PluginLog.Debug("Party Wipe | DutyEventFunction");
                        Wipe();
                        break;

                    // Duty Recommence
                    case 0x40000006:
                        PluginLog.Debug("Duty Recommence | DutyEventFunction");
                        dutyManager.StartNewRun();
                        break;

                    // Duty Completed
                    case 0x40000003:
                        PluginLog.Debug("Duty Completed | DutyEventFunction");
                        StopDuty();
                        completedThisTerritory = true;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Caught unexpected exception in duty event manager");
        }

        return DutyEventHook!.Original(a1, a2, a3);
    }


    private void Wipe()
    {
        dutyManager.EndRun();
    }

    private void StartDuty()
    {
        DutyStarted = true;
        dutyManager.StartDuty();
    }

    private void StopDuty()
    {
        DutyStarted = false;
        dutyManager.EndDuty();
    }

    private bool IsBoundByDuty()
    {
        var baseBoundByDuty = Service.Condition[ConditionFlag.BoundByDuty];
        var boundBy56       = Service.Condition[ConditionFlag.BoundByDuty56];
        var boundBy95       = Service.Condition[ConditionFlag.BoundByDuty95];

        return baseBoundByDuty || boundBy56 || boundBy95;
    }
}
