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

    [PluginService] public Condition   Condition   { get; init; }
    [PluginService] public Framework   Framework   { get; init; }
    [PluginService] public ClientState ClientState { get; init; }
    [PluginService] public ChatGui     ChatGui     { get; init; }
    public                 bool        DutyStarted { get; private set; }
    public                 int         Wipes       { get; set; }
    public                 int         Deaths      { get; set; }
    private                bool        CompletedThisTerritory;

    public DutyEventManager()
    {
        SignatureHelper.Initialise(this);

        DutyEventHook?.Enable();

        if (IsBoundByDuty())
        {
            StartDuty();
        }

        Framework!.Update             += FrameworkUpdate;
        ClientState!.TerritoryChanged += TerritoryChanged;
        
        PluginLog.Debug("DutyEventManager initialized");
    }

    public void Dispose()
    {
        DutyEventHook?.Dispose();

        Framework.Update             -= FrameworkUpdate;
        ClientState.TerritoryChanged -= TerritoryChanged;
    }

    private void FrameworkUpdate(Framework framework)
    {
        if (!DutyStarted && !CompletedThisTerritory)
        {
            if (IsBoundByDuty() && Condition[ConditionFlag.InCombat])
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

        CompletedThisTerritory = false;
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
                        AddWipe();
                        break;

                    // Duty Recommence
                    case 0x40000006:
                        PluginLog.Debug("Duty Recommence | DutyEventFunction");
                        StartDuty();
                        break;

                    // Duty Completed
                    case 0x40000003:
                        PluginLog.Debug("Duty Completed | DutyEventFunction");
                        StopDuty();
                        CompletedThisTerritory = true;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed to get Duty Started Status");
        }

        return DutyEventHook!.Original(a1, a2, a3);
    }


    private void AddWipe()
    {
        Wipes++;
        PluginLog.Verbose($"Wipe detected. Total wipes = {Wipes.ToString()}");
    }
    
    private void StartDuty()
    {
        DutyStarted = true;
        StartTime   = DateTime.Now;
        PluginLog.Verbose($"Start Time: {StartTime}");
    }

    private void StopDuty()
    {
        DutyStarted = false;
        EndTime     = DateTime.Now;
        PluginLog.Verbose($"End Time: {EndTime}");
        ChatGui.Print(InfoMessage("Completion Time: ", $"{ElapsedTime:m\\:ss}"));
        ChatGui.Print(InfoMessage("Deaths: ",          Deaths.ToString()));
        ChatGui.Print(InfoMessage("Wipes: ",           Wipes.ToString()));
        Wipes  = 0;
        Deaths = 0;
    }

    private static SeString InfoMessage(string label, string info)
    {
        return new SeStringBuilder()
              .AddUiForeground("[DutyTracker] ", 35)
              .AddUiForeground(label,            62)
              .AddUiForeground(info,             45)
              .Build();
    }

    private bool IsBoundByDuty()
    {
        var baseBoundByDuty = Condition[ConditionFlag.BoundByDuty];
        var boundBy56       = Condition[ConditionFlag.BoundByDuty56];
        var boundBy95       = Condition[ConditionFlag.BoundByDuty95];

        return baseBoundByDuty || boundBy56 || boundBy95;
    }
}
