using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using DutyTracker.Duty_Events;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace DutyTracker;

// Adapted from DeathRecap (https://github.com/Kouzukii/ffxiv-deathrecap)
// https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Events/CombatEventCapture.cs

public class CombatEventCapture : IDisposable
{
    private readonly ObjectTable ObjectTable;
    private readonly PartyList   PartyList;
    private readonly DutyManager DutyManager;

    private delegate void ReceiveActorControlSelfDelegate(
        uint entityId, uint id, uint arg0, uint arg1, uint arg2, uint arg3, uint arg4, uint arg5,
        ulong targetId, byte a10);

    // ffxiv_dx11.exe+6C3ED9 - E8 526B0600           - call ffxiv_dx11.exe+72AA30
    // ffxiv_dx11.exe+6C3EDE - 0FB7 0B               - movzx ecx,word ptr [rbx]
    // ffxiv_dx11.exe+6C3EE1 - 83 E9 64              - sub ecx,64 { 100 }
    // ffxiv_dx11.exe+6C3EE4 - 0F84 F3000000         - je ffxiv_dx11.exe+6C3FDD
    // ffxiv_dx11.exe+6C3EEA - 83 E9 01              - sub ecx,01 { 1 }
    // ffxiv_dx11.exe+6C3EED - 0F84 C9000000         - je ffxiv_dx11.exe+6C3FBC
    // ffxiv_dx11.exe+6C3EF3 - 83 E9 08              - sub ecx,08 { 8 }
    // ffxiv_dx11.exe+6C3EF6 - 74 29                 - je ffxiv_dx11.exe+6C3F21
    // ffxiv_dx11.exe+6C3EF8 - 81 F9 39030000        - cmp ecx,00000339 { 825 }
    // ffxiv_dx11.exe+6C3EFE - 0F85 19010000         - jne ffxiv_dx11.exe+6C401D
    // ffxiv_dx11.exe+6C3F04 - 0FB7 43 04            - movzx eax,word ptr [rbx+04]
    // ffxiv_dx11.exe+6C3F08 - 66 89 86 10070000     - mov [rsi+00000710],ax
    // ffxiv_dx11.exe+6C3F0F - B0 01                 - mov al,01 { 1 }
    // ffxiv_dx11.exe+6C3F11 - 48 8B 5C 24 60        - mov rbx,[rsp+60]
    // ffxiv_dx11.exe+6C3F16 - 48 8B 74 24 68        - mov rsi,[rsp+68]
    // ffxiv_dx11.exe+6C3F1B - 48 83 C4 50           - add rsp,50 { 80 }
    // ffxiv_dx11.exe+6C3F1F - 5F                    - pop rdi
    // ffxiv_dx11.exe+6C3F20 - C3                    - ret 
    // ffxiv_dx11.exe+6C3F21 - 8B 86 28060000        - mov eax,[rsi+00000628]
    // ffxiv_dx11.exe+6C3F27 - 85 C0                 - test eax,eax
    // ffxiv_dx11.exe+6C3F29 - 0F84 EE000000         - je ffxiv_dx11.exe+6C401D
    // ffxiv_dx11.exe+6C3F2F - 3B 43 04              - cmp eax,[rbx+04]
    // ffxiv_dx11.exe+6C3F32 - 0F85 E5000000         - jne ffxiv_dx11.exe+6C401D
    // ffxiv_dx11.exe+6C3F38 - C1 E8 10              - shr eax,10 { 16 }
    [Signature("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", DetourName = nameof(ReceiveActorControlSelfDetour))]
    private readonly Hook<ReceiveActorControlSelfDelegate> receiveActorControlSelfHook = null!;

    private const ushort ActorControlDeathCode = 0x06;

    public CombatEventCapture(
        [RequiredVersion("1.0")] ObjectTable objectTable,
        [RequiredVersion("1.0")] PartyList   partyList,
        DutyManager                          dutyManager)
    {
        ObjectTable = objectTable;
        PartyList   = partyList;
        DutyManager = dutyManager;

        SignatureHelper.Initialise(this);

        receiveActorControlSelfHook.Enable();

        PluginLog.Debug("CombatEventCapture initialized.");
    }

    private bool LookupPartyMember(uint actorId)
    {
        var count = PartyList.IsAlliance ? 8 : 32;

        for (var i = 0; i < count; i++)
            if (PartyList[i]?.ObjectId is { } id)
                if (actorId == id)
                    return true;

        return false;
    }

    public bool ShouldCapture(uint actorId)
    {
        return actorId == ObjectTable[0]?.ObjectId || LookupPartyMember(actorId);
    }

    private void ReceiveActorControlSelfDetour(
        uint  entityId,
        uint  type,
        uint  buffId,
        uint  param1,
        uint  param2,
        uint  sourceId,
        uint  arg4,
        uint  arg5,
        ulong targetId,
        byte  a10)
    {
        receiveActorControlSelfHook.Original(entityId, type, buffId, param1, param2, sourceId, arg4, arg5, targetId, a10);

        try
        {
            if (!ShouldCapture(entityId))
                return;

            if (ObjectTable.SearchById(entityId) is not PlayerCharacter p)
                return;
            if (type == ActorControlDeathCode)
            {
                PluginLog.Debug("Death Event detected.");
                DutyManager.AddDeathEvent(new DeathEvent
                                          {
                                              PlayerId    = entityId,
                                              PlayerName  = p.Name.TextValue,
                                              TimeOfDeath = DateTime.Now,
                                          });
            }
        }
        catch (Exception e)
        {
            PluginLog.Error(e, "Caught unexpected exception in combat event capture");
        }
    }
    
    public void Dispose() {
        receiveActorControlSelfHook.Disable();
    }
}
