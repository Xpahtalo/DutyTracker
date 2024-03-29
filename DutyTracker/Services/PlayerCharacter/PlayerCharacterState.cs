﻿using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin.Services;
using DutyTracker.Enums;
using DutyTracker.Services.DutyEvent;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using XpahtaLib.DalamudUtilities.UsefulEnums;

namespace DutyTracker.Services.PlayerCharacter;

public sealed unsafe class PlayerCharacterState : IDisposable
{
    private readonly IFramework           _framework;
    private readonly DutyEventService     _dutyEventService;
    private readonly CachedPartyMember?[] _partyCache;
    private readonly CachedPartyMember?[] _allianceCache;

    private AllianceState _allianceState;
    private PartyState    _partyState;

    private AllianceType _allianceType;
    private Alliance     _partyAlliance;
    private Alliance     _alliance1;
    private Alliance     _alliance2;
    private Alliance     _alliance3;
    private Alliance     _alliance4;
    private Alliance     _alliance5;

    // These are all magic numbers that correspond to memory locations in the game.
    private const int AllianceStringPosition = 64;
    private const int Party1Position         = 0;
    private const int Party2Position         = 9;
    private const int Party3Position         = 18;
    private const int Party4Position         = 27;
    private const int Party5Position         = 36;
    private const int AllianceSize           = 40;
    private const int PartySize              = 8;

    public event EventHandler<PlayerDeathEventArgs>? OnPlayerDeath;

    public PlayerCharacterState()
    {
        _framework        = Service.Framework;
        _dutyEventService = Service.DutyEventService;

        _allianceState = AllianceState.NoGroup;
        _partyCache    = new CachedPartyMember?[PartySize];
        _allianceCache = new CachedPartyMember?[AllianceSize];

        _framework.Update             += FrameworkUpdate;
        _dutyEventService.DutyStarted += DutyStarted;
        _dutyEventService.DutyEnded   += DutyEnded;
        Service.PluginLog.Debug("PlayerCharacterState initialized.");
    }

    public void Dispose()
    {
        _framework.Update             -= FrameworkUpdate;
        _dutyEventService.DutyStarted -= DutyStarted;
        _dutyEventService.DutyEnded   -= DutyEnded;
    }

    private void FrameworkUpdate(IFramework framework)
    {
        var groupManager = GroupManager.Instance();

        switch (_allianceState) {
            case AllianceState.WaitingForData:
                // Need the alliance string array to be populated to determine which parties are which alliance,
                // but it is not populated until the game is ready to draw the alliance list.
                if (IsAllianceStringDataPopulated()) {
                    SetAlliances(groupManager);
                    _allianceState = AllianceState.Running;
                    _partyState    = PartyState.Running;
                    Service.PluginLog.Debug("Alliance data detected. Changing state to Running.");
                }

                break;
            case AllianceState.Running:
                UpdateCache(_allianceCache, groupManager->GetAllianceMemberByIndex, index =>
                {
                    switch (_allianceType) {
                        case AllianceType.None:
                            Service.PluginLog.Debug("  No alliance.");
                            return Alliance.None;
                        case AllianceType.ThreeParty:
                            Service.PluginLog.Debug($"  Index = {index}, Alliance = {index / 8}");
                            return (index / 8) switch
                            {
                                0 => _alliance1,
                                1 => _alliance2,
                                _ => Alliance.None,
                            };
                        case AllianceType.SixParty:
                            Service.PluginLog.Debug($"  Index = {index}, Alliance = {index / 4}");
                            return (index / 4) switch
                            {
                                0 => _alliance1,
                                1 => _alliance2,
                                2 => _alliance3,
                                3 => _alliance4,
                                4 => _alliance5,
                                _ => Alliance.None,
                            };
                        default:
                            throw new ArgumentOutOfRangeException("");
                    }
                });
                break;
        }

        if (_partyState == PartyState.Running)
            UpdateCache(_partyCache, groupManager->GetPartyMemberByIndex, _ => _partyAlliance);
    }

    private void DutyStarted(object? sender, DutyStartedEventArgs eventArgs)
    {
        if (eventArgs.IntendedUse.HasAlliance()) {
            _allianceState = AllianceState.WaitingForData;
            _partyState    = PartyState.WaitingForAlliance;
        } else {
            _partyState = PartyState.Running;
        }
    }

    private void DutyEnded(object? sender, DutyEndedEventArgs eventArgs)
    {
        _allianceState = AllianceState.NoGroup;
        _partyState    = PartyState.NoGroup;
        ResetAllianceInfo();
        ResetPartyInfo();
    }

    private static bool IsAllianceStringDataPopulated()
    {
        var stringArray =
            Framework.Instance()->GetUiModule()->GetRaptureAtkModule()
                ->AtkModule.AtkArrayDataHolder.StringArrays[AllianceStringPosition]->StringArray;

        return !string.IsNullOrWhiteSpace(Marshal.PtrToStringUTF8(new nint(stringArray[0])));
    }

    private void SetAlliances(GroupManager* groupManager)
    {
        var stringArray =
            Framework.Instance()->GetUiModule()->GetRaptureAtkModule()
                ->AtkModule.AtkArrayDataHolder.StringArrays[AllianceStringPosition]->StringArray;
        Service.PluginLog.Debug("Party Strings");
        Service.PluginLog.Debug($"  string{Party1Position}:  {Marshal.PtrToStringUTF8(new nint(stringArray[Party1Position]))}");
        Service.PluginLog.Debug($"  string{Party2Position}:  {Marshal.PtrToStringUTF8(new nint(stringArray[Party2Position]))}");
        Service.PluginLog.Debug($"  string{Party3Position}: {Marshal.PtrToStringUTF8(new nint(stringArray[Party3Position]))}");
        Service.PluginLog.Debug($"  string{Party4Position}: {Marshal.PtrToStringUTF8(new nint(stringArray[Party4Position]))}");
        Service.PluginLog.Debug($"  string{Party5Position}: {Marshal.PtrToStringUTF8(new nint(stringArray[Party5Position]))}");

        _alliance1 = AllianceExtensions.ToAlliance(stringArray[Party1Position]);
        _alliance2 = AllianceExtensions.ToAlliance(stringArray[Party2Position]);
        _alliance3 = AllianceExtensions.ToAlliance(stringArray[Party3Position]);
        _alliance4 = AllianceExtensions.ToAlliance(stringArray[Party4Position]);
        _alliance5 = AllianceExtensions.ToAlliance(stringArray[Party5Position]);

        _allianceType = (AllianceType)groupManager->AllianceFlags;

        _partyAlliance = _allianceType switch
        {
            AllianceType.ThreeParty => (alliance1: _alliance1, alliance2: _alliance2) switch
            {
                (Alliance.A, Alliance.B) => Alliance.C,
                (Alliance.A, Alliance.C) => Alliance.B,
                (Alliance.B, Alliance.C) => Alliance.A,
                (_, _)                   => Alliance.None,
            },
            AllianceType.SixParty => (alliance1: _alliance1, alliance2: _alliance2, alliance3: _alliance3, alliance4: _alliance4, alliance5: _alliance5) switch
            {
                (Alliance.A, Alliance.B, Alliance.C, Alliance.D, Alliance.E) => Alliance.F,
                (Alliance.A, Alliance.B, Alliance.C, Alliance.D, Alliance.F) => Alliance.E,
                (Alliance.A, Alliance.B, Alliance.C, Alliance.E, Alliance.F) => Alliance.D,
                (Alliance.A, Alliance.B, Alliance.D, Alliance.E, Alliance.F) => Alliance.C,
                (Alliance.A, Alliance.C, Alliance.D, Alliance.E, Alliance.F) => Alliance.B,
                (Alliance.B, Alliance.C, Alliance.D, Alliance.E, Alliance.F) => Alliance.A,
                (_, _, _, _, _)                                              => Alliance.None,
            },
            _ => Alliance.None,
        };

        Service.PluginLog.Verbose($"Alliance Type: {_allianceType}");
        Service.PluginLog.Verbose($"Party Alliance: {_partyAlliance}");
        Service.PluginLog.Verbose($"Alliance Parties: {_alliance1}, {_alliance2}, {_alliance3}, {_alliance4}, {_alliance5}");
    }

    private void ResetAllianceInfo()
    {
        for (var i = 0; i < _allianceCache.Length; i++) {
            _allianceCache[i] = null;
        }

        _alliance1 = Alliance.None;
        _alliance2 = Alliance.None;
        _alliance3 = Alliance.None;
        _alliance4 = Alliance.None;
        _alliance5 = Alliance.None;
    }

    private void ResetPartyInfo()
    {
        for (var i = 0; i < _partyCache.Length; i++) {
            _partyCache[i] = null;
        }

        _partyAlliance = Alliance.None;
    }


    // This is a bit messy, but passing in the array and access functions lets me keep the actual logic the exact same 
    // between party and alliance members, while still tracking them separately. It was especially difficult to work
    // with when doing it separately due to code duplication with slight differences.
    private delegate PartyMember* GetPartyMember(int index);

    private delegate Alliance GetAlliance(int index);

    private void UpdateCache(CachedPartyMember?[] cache, GetPartyMember getMember, GetAlliance getAlliance)
    {
        for (var i = 0; i < cache.Length; i++) {
            var partyMember  = getMember(i);
            var playerExists = IsPlayerInitialized(partyMember);

            if (cache[i] is not null) {
                // Have to check if the player exists first, because the health check will be true if they don't.
                // This would result in player being marked dead instead of leaving.
                if (!playerExists) {
                    PlayerLeft(i);
                } else {
                    if (cache[i]!.Hp != partyMember->CurrentHP) {
                        cache[i]!.Hp = partyMember->CurrentHP;

                        if (cache[i]!.Hp == 0) PlayerDied(i);
                    }
                }
            } else {
                if (playerExists) AddPlayer(partyMember, getAlliance(i), i);
            }
        }

        void PlayerLeft(int i)
        {
            Service.PluginLog.Debug($"Player left: {i}, {cache[i]!.Name}, {cache[i]!.ObjectId}");
            cache[i] = null;
        }

        void PlayerDied(int i)
        {
            var playerName = cache[i]!.Name;
            var objectId   = cache[i]!.ObjectId;
            var alliance   = cache[i]!.Alliance;

            Service.PluginLog.Debug($"Played died: {i}, {playerName}, {objectId}, {alliance}");
            OnPlayerDeath?.Invoke(this, new PlayerDeathEventArgs(playerName, objectId, alliance));
        }

        void AddPlayer(PartyMember* partyMember, Alliance alliance, int i)
        {
            var newPlayer = new CachedPartyMember(GetPlayerName(partyMember), partyMember->ObjectID, partyMember->CurrentHP, alliance);
            Service.PluginLog.Debug($"Detected new player: {i}, {newPlayer}");
            cache[i] = newPlayer;
        }
    }

    private static GroupManager* GetSecondGroupManager(GroupManager* firstGroupManager) => firstGroupManager + 1;

    private static string GetPlayerName(PartyMember* partyMember) => Marshal.PtrToStringUTF8(new nint(partyMember->Name)) ?? string.Empty;

    /// <summary>
    ///     <see cref="GroupManager.GetPartyMemberByIndex" /> returns only a pointer. SE pointers can be null, or 0xE0000000
    ///     if they have cleared the memory.
    /// </summary>
    /// <param name="partyMember">A pointer to the <see cref="PartyMember">party member</see>.</param>
    /// <returns>true if the pointer has been initialized to a valid <see cref="PartyMember" />, otherwise false.</returns>
    private static bool IsPlayerInitialized(PartyMember* partyMember)
    {
        const uint seUninitializedPointer = 0xE0000000;

        if (partyMember is null)
            return false;
        if (partyMember->ObjectID == seUninitializedPointer)
            return false;

        return true;
    }

#region DebugInfo

    public static void DebugGroupManager()
    {
        var groupManager1 = GroupManager.Instance();
        var groupManager2 = GetSecondGroupManager(groupManager1);
        var index         = 1;

        if (groupManager1 is null) {
            ImGui.Text("No Group Manager Available.");
            return;
        }

        ImGui.Text("== GroupManager 1 ==");
        ImGuiGroupManager(groupManager1);
        ImGuiParty(groupManager1);
        ImGuiAlliance(groupManager1);
        index++;
        ImGui.Text("== GroupManager 2 ==");
        ImGuiGroupManager(groupManager2);
        ImGuiParty(groupManager2);
        ImGuiAlliance(groupManager2);


        void ImGuiGroupManager(GroupManager* groupManager)
        {
            ImGui.Text($"Alliance Flags: {groupManager->AllianceFlags}");
            ImGui.Text($"MemberCount: {groupManager->MemberCount}");
            ImGui.Text($"PartyId: {groupManager->PartyId}");
            ImGui.Text($"PartyId_2: {groupManager->PartyId_2}");
            ImGui.Text($"PartyLeaderIndex: {groupManager->PartyLeaderIndex}");
            ImGui.Text($"Unk_3D40: {groupManager->Unk_3D40}");
            ImGui.Text($"Unk_3D44: {groupManager->Unk_3D44}");
            ImGui.Text($"Unk_3D5D: {groupManager->Unk_3D5D}");
            ImGui.Text($"Unk_3D5F: {groupManager->Unk_3D5F}");
            ImGui.Text($"Unk_3D60: {groupManager->Unk_3D60}");
        }

        void ImGuiParty(GroupManager* groupManager)
        {
            if (groupManager->MemberCount == 0) {
                ImGui.Text(" - No Party - ");
                return;
            }

            ImGui.Text("Party Members:");
            if (ImGui.BeginTable($"Party#{index}", 5)) {
                XGui.TableHeader("Index", "Name", "ObjectID", "CurrentHP", "ClassJob");

                for (var i = 0; i < 8; i++) {
                    var partyMember = groupManager->GetPartyMemberByIndex(i);
                    if (IsPlayerInitialized(partyMember))
                        DebugPartyMemberRow(partyMember, i);
                    else
                        EmptyPlayerRow(i);
                }
            }

            ImGui.EndTable();
        }

        void ImGuiAlliance(GroupManager* groupManager)
        {
            if (groupManager->AllianceFlags == 0x00) {
                ImGui.Text(" - No Alliance - ");
                return;
            }

            ImGui.Text("Alliance Members:");
            if (ImGui.BeginTable($"Alliance#{index}", 5)) {
                XGui.TableHeader("Index", "Name", "ObjectID", "CurrentHP", "ClassJob");
                for (var i = 0; i < 20; i++) {
                    var allianceMember = groupManager->GetAllianceMemberByIndex(i);
                    if (IsPlayerInitialized(allianceMember))
                        try {
                            DebugPartyMemberRow(allianceMember, i);
                        } catch (Exception ex) {
                            Service.PluginLog.Error($"{ex}");
                            XGui.TableRow($"{i}",
                                          "error",
                                          "error",
                                          "error",
                                          "error");
                        }
                    else
                        XGui.TableRow($"{i}",
                                      "-",
                                      "-",
                                      "-",
                                      "-");
                }
            }

            ImGui.EndTable();
        }
    }

    private static void DebugPartyMemberRow(PartyMember* partyMember, int i)
    {
        XGui.TableRow($"{i}",
                      $"{Marshal.PtrToStringUTF8(new nint(partyMember->Name))}",
                      $"{partyMember->ObjectID}",
                      $"{partyMember->CurrentHP}",
                      $"{partyMember->ClassJob}");
    }

    public void DebugCache()
    {
        ImGui.Text($"Party Alliance = {_partyAlliance}");
        ImGui.Text($"Alliance1 = {_alliance1}");
        ImGui.Text($"Alliance2 = {_alliance2}");
        ImGui.Text($"Alliance3 = {_alliance3}");
        ImGui.Text($"Alliance4 = {_alliance4}");
        ImGui.Text($"Alliance5 = {_alliance5}");

        ImGui.Text($"Party State = {_partyState}");
        ImGui.Text($"Alliance State = {_allianceState}");

        ImGui.Text("Party Cache");
        if (ImGui.BeginTable("PartyCache", 5)) {
            XGui.TableHeader("", "Name", "ObjectID", "HP", "Alliance");
            for (var i = 0; i < _partyCache.Length; i++) {
                var cachedMember = _partyCache[i];
                if (cachedMember is not null)
                    XGui.TableRow($"{i}",
                                  $"{cachedMember.Name}",
                                  $"{cachedMember.ObjectId}",
                                  $"{cachedMember.Hp}",
                                  $"{cachedMember.Alliance}");
                else
                    EmptyPlayerRow(i);
            }
        }

        ImGui.EndTable();

        ImGui.Text("Alliance Cache");
        if (ImGui.BeginTable("AllianceCache", 5)) {
            XGui.TableHeader("", "Name", "ObjectID", "HP", "Alliance");
            for (var i = 0; i < _allianceCache.Length; i++) {
                var cachedMember = _allianceCache[i];
                if (cachedMember is not null)
                    XGui.TableRow($"{i}",
                                  $"{cachedMember.Name}",
                                  $"{cachedMember.ObjectId}",
                                  $"{cachedMember.Hp}",
                                  $"{cachedMember.Alliance}");
                else
                    EmptyPlayerRow(i);
            }
        }

        ImGui.EndTable();
    }

    private static void EmptyPlayerRow(int i) { XGui.TableRow($"{i}", "-", "-", "-", "-"); }

#endregion
}
