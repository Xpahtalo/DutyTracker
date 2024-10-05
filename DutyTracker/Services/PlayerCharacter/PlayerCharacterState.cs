using System;
using System.Runtime.InteropServices;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using DutyTracker.Enums;
using DutyTracker.Extensions;
using DutyTracker.Services.DutyEvent;
using DutyTracker.Windows;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;

namespace DutyTracker.Services.PlayerCharacter;

public sealed unsafe class PlayerCharacterState : IDisposable
{
    private readonly CachedPartyMember?[] PartyCache;
    private readonly CachedPartyMember?[] AllianceCache;

    private AllianceState AllianceState;
    private PartyState PartyState;

    private AllianceType AllianceType;
    private Alliance PartyAlliance;
    private Alliance Alliance1;
    private Alliance Alliance2;
    private Alliance Alliance3;
    private Alliance Alliance4;
    private Alliance Alliance5;

    // These are all magic numbers that correspond to memory locations in the game.
    private const int AllianceStringPosition = 64;
    private const int Party1Position = 0;
    private const int Party2Position = 9;
    private const int Party3Position = 18;
    private const int Party4Position = 27;
    private const int Party5Position = 36;

    public event EventHandler<PlayerDeathEventArgs>? OnPlayerDeath;

    public PlayerCharacterState()
    {
        AllianceState = AllianceState.NoGroup;
        PartyCache = new CachedPartyMember?[8];
        AllianceCache = new CachedPartyMember?[40];

        DutyTracker.Framework.Update += FrameworkUpdate;
        DutyTracker.DutyEventService.OnDutyStartedEvent += DutyStarted;
        DutyTracker.DutyEventService.OnDutyEndedEvent += DutyEnded;
        DutyTracker.Log.Debug("PlayerCharacterState initialized.");
    }

    public void Dispose()
    {
        DutyTracker.Framework.Update -= FrameworkUpdate;
        DutyTracker.DutyEventService.OnDutyStartedEvent -= DutyStarted;
        DutyTracker.DutyEventService.OnDutyEndedEvent -= DutyEnded;
    }

    private void FrameworkUpdate(IFramework framework)
    {
        var groupManager = GroupManager.Instance();

        switch (AllianceState)
        {
            case AllianceState.WaitingForData:
                // Need the alliance string array to be populated to determine which parties are which alliance,
                // but it is not populated until the game is ready to draw the alliance list.
                if (IsAllianceStringDataPopulated())
                {
                    SetAlliances(groupManager);
                    AllianceState = AllianceState.Running;
                    PartyState = PartyState.Running;
                    DutyTracker.Log.Debug("Alliance data detected. Changing state to Running.");
                }

                break;
            case AllianceState.Running:
                UpdateCache(AllianceCache, groupManager->MainGroup.GetAllianceMemberByIndex, index =>
                {
                    switch (AllianceType)
                    {
                        case AllianceType.None:
                            DutyTracker.Log.Debug("  No alliance.");
                            return Alliance.None;
                        case AllianceType.ThreeParty:
                            DutyTracker.Log.Debug($"  Index = {index}, Alliance = {index / 8}");
                            return (index / 8) switch
                            {
                                0 => Alliance1,
                                1 => Alliance2,
                                _ => Alliance.None,
                            };
                        case AllianceType.SixParty:
                            DutyTracker.Log.Debug($"  Index = {index}, Alliance = {index / 4}");
                            return (index / 4) switch
                            {
                                0 => Alliance1,
                                1 => Alliance2,
                                2 => Alliance3,
                                3 => Alliance4,
                                4 => Alliance5,
                                _ => Alliance.None,
                            };
                        default:
                            throw new ArgumentOutOfRangeException("");
                    }
                });
                break;
        }

        if (PartyState == PartyState.Running)
            UpdateCache(PartyCache, groupManager->MainGroup.GetPartyMemberByIndex, _ => PartyAlliance);
    }

    private void DutyStarted(object? sender, DutyStartedEventArgs eventArgs)
    {
        if (eventArgs.IntendedUse.HasAlliance())
        {
            AllianceState = AllianceState.WaitingForData;
            PartyState = PartyState.WaitingForAlliance;
        }
        else
        {
            PartyState = PartyState.Running;
        }
    }

    private void DutyEnded(object? sender, DutyEndedEventArgs eventArgs)
    {
        AllianceState = AllianceState.NoGroup;
        PartyState = PartyState.NoGroup;
        ResetAllianceInfo();
        ResetPartyInfo();
    }

    private static bool IsAllianceStringDataPopulated()
    {
        var stringArray = RaptureAtkModule.Instance()->AtkModule.AtkArrayDataHolder.StringArrays[AllianceStringPosition]->StringArray;

        return !string.IsNullOrWhiteSpace(Marshal.PtrToStringUTF8(new nint(stringArray[0])));
    }

    private void SetAlliances(GroupManager* groupManager)
    {
        var stringArray = RaptureAtkModule.Instance()->AtkModule.AtkArrayDataHolder.StringArrays[AllianceStringPosition]->StringArray;
        DutyTracker.Log.Debug("Party Strings");
        DutyTracker.Log.Debug($"  string{Party1Position}:  {Marshal.PtrToStringUTF8(new nint(stringArray[Party1Position]))}");
        DutyTracker.Log.Debug($"  string{Party2Position}:  {Marshal.PtrToStringUTF8(new nint(stringArray[Party2Position]))}");
        DutyTracker.Log.Debug($"  string{Party3Position}: {Marshal.PtrToStringUTF8(new nint(stringArray[Party3Position]))}");
        DutyTracker.Log.Debug($"  string{Party4Position}: {Marshal.PtrToStringUTF8(new nint(stringArray[Party4Position]))}");
        DutyTracker.Log.Debug($"  string{Party5Position}: {Marshal.PtrToStringUTF8(new nint(stringArray[Party5Position]))}");

        Alliance1 = AllianceExtensions.ToAlliance(stringArray[Party1Position]);
        Alliance2 = AllianceExtensions.ToAlliance(stringArray[Party2Position]);
        Alliance3 = AllianceExtensions.ToAlliance(stringArray[Party3Position]);
        Alliance4 = AllianceExtensions.ToAlliance(stringArray[Party4Position]);
        Alliance5 = AllianceExtensions.ToAlliance(stringArray[Party5Position]);

        AllianceType = (AllianceType)groupManager->MainGroup.AllianceFlags;
        PartyAlliance = AllianceType switch
        {
            AllianceType.ThreeParty => (alliance1: Alliance1, alliance2: Alliance2) switch
            {
                (Alliance.A, Alliance.B) => Alliance.C,
                (Alliance.A, Alliance.C) => Alliance.B,
                (Alliance.B, Alliance.C) => Alliance.A,
                (_, _) => Alliance.None,
            },
            AllianceType.SixParty => (alliance1: Alliance1, alliance2: Alliance2, alliance3: Alliance3, alliance4: Alliance4, alliance5: Alliance5) switch
                {
                    (Alliance.A, Alliance.B, Alliance.C, Alliance.D, Alliance.E) => Alliance.F,
                    (Alliance.A, Alliance.B, Alliance.C, Alliance.D, Alliance.F) => Alliance.E,
                    (Alliance.A, Alliance.B, Alliance.C, Alliance.E, Alliance.F) => Alliance.D,
                    (Alliance.A, Alliance.B, Alliance.D, Alliance.E, Alliance.F) => Alliance.C,
                    (Alliance.A, Alliance.C, Alliance.D, Alliance.E, Alliance.F) => Alliance.B,
                    (Alliance.B, Alliance.C, Alliance.D, Alliance.E, Alliance.F) => Alliance.A,
                    (_, _, _, _, _) => Alliance.None,
                },
            _ => Alliance.None,
        };

        DutyTracker.Log.Verbose($"Alliance Type: {AllianceType}");
        DutyTracker.Log.Verbose($"Party Alliance: {PartyAlliance}");
        DutyTracker.Log.Verbose($"Alliance Parties: {Alliance1}, {Alliance2}, {Alliance3}, {Alliance4}, {Alliance5}");
    }

    private void ResetAllianceInfo()
    {
        for (var i = 0; i < AllianceCache.Length; i++)
            AllianceCache[i] = null;

        Alliance1 = Alliance.None;
        Alliance2 = Alliance.None;
        Alliance3 = Alliance.None;
        Alliance4 = Alliance.None;
        Alliance5 = Alliance.None;
    }

    private void ResetPartyInfo()
    {
        for (var i = 0; i < PartyCache.Length; i++)
            PartyCache[i] = null;

        PartyAlliance = Alliance.None;
    }


    // This is a bit messy, but passing in the array and access functions lets me keep the actual logic the exact same
    // between party and alliance members, while still tracking them separately. It was especially difficult to work
    // with when doing it separately due to code duplication with slight differences.
    private delegate PartyMember* GetPartyMember(int index);
    private delegate Alliance GetAlliance(int index);

    private void UpdateCache(CachedPartyMember?[] cache, GetPartyMember getMember, GetAlliance getAlliance)
    {
        for (var i = 0; i < cache.Length; i++)
        {
            var partyMember = getMember(i);
            var playerExists = IsPlayerInitialized(partyMember);

            if (cache[i] is not null)
            {
                // Have to check if the player exists first, because the health check will be true if they don't.
                // This would result in player being marked dead instead of leaving.
                if (!playerExists)
                {
                    PlayerLeft(i);
                }
                else
                {
                    if (cache[i]!.Hp != partyMember->CurrentHP)
                    {
                        cache[i]!.Hp = partyMember->CurrentHP;
                        if (cache[i]!.Hp == 0)
                            PlayerDied(i);
                    }
                }
            }
            else
            {
                if (playerExists) AddPlayer(partyMember, getAlliance(i), i);
            }
        }

        return;

        void PlayerLeft(int i)
        {
            DutyTracker.Log.Debug($"Player left: {i}, {cache[i]!.Name}");
            cache[i] = null;
        }

        void PlayerDied(int i)
        {
            var playerName = cache[i]!.Name;
            var alliance = cache[i]!.Alliance;

            DutyTracker.Log.Debug($"Played died: {i}, {playerName}, {alliance}");
            OnPlayerDeath?.Invoke(this, new PlayerDeathEventArgs(playerName, alliance));
        }

        void AddPlayer(PartyMember* partyMember, Alliance alliance, int i)
        {
            var newPlayer = new CachedPartyMember(GetPlayerName(partyMember), partyMember->CurrentHP, alliance);
            DutyTracker.Log.Debug($"Detected new player: {i}, {newPlayer}");
            cache[i] = newPlayer;
        }
    }

    private static GroupManager* GetSecondGroupManager(GroupManager* firstGroupManager) => firstGroupManager + 1;

    private static string GetPlayerName(PartyMember* partyMember) => partyMember->NameString ?? string.Empty;

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
        if (partyMember->EntityId == seUninitializedPointer)
            return false;

        return true;
    }

    #region DebugInfo

    public static void DebugGroupManager()
    {
        var groupManager1 = GroupManager.Instance();
        var groupManager2 = GetSecondGroupManager(groupManager1);
        var index = 1;

        if (groupManager1 is null)
        {
            ImGui.TextUnformatted("No Group Manager Available.");
            return;
        }

        ImGui.TextUnformatted("== GroupManager 1 ==");
        ImGuiGroupManager(groupManager1);
        ImGuiParty(groupManager1);
        ImGuiAlliance(groupManager1);
        index++;
        ImGui.TextUnformatted("== GroupManager 2 ==");
        ImGuiGroupManager(groupManager2);
        ImGuiParty(groupManager2);
        ImGuiAlliance(groupManager2);

        return;

        void ImGuiGroupManager(GroupManager* groupManager)
        {
            ImGui.TextUnformatted($"Alliance Flags: {groupManager->MainGroup.AllianceFlags}");
            ImGui.TextUnformatted($"MemberCount: {groupManager->MainGroup.MemberCount}");
            ImGui.TextUnformatted($"PartyId: {groupManager->MainGroup.PartyId}");
            ImGui.TextUnformatted($"PartyId_2: {groupManager->MainGroup.PartyId_2}");
            ImGui.TextUnformatted($"PartyLeaderIndex: {groupManager->MainGroup.PartyLeaderIndex}");
        }

        void ImGuiParty(GroupManager* groupManager)
        {
            if (groupManager->MainGroup.MemberCount == 0)
            {
                ImGui.TextUnformatted(" - No Party - ");
                return;
            }

            ImGui.TextUnformatted("Party Members:");
            using var table = ImRaii.Table($"Party#{index}", 4);
            if (!table.Success)
                return;

            Helper.TableHeader("Index", "Name", "CurrentHP", "ClassJob");
            for (var i = 0; i < 8; i++)
            {
                var partyMember = groupManager->MainGroup.GetPartyMemberByIndex(i);
                if (IsPlayerInitialized(partyMember))
                    DebugPartyMemberRow(partyMember, i);
                else
                    EmptyPlayerRow(i);
            }
        }

        void ImGuiAlliance(GroupManager* groupManager)
        {
            if (groupManager->MainGroup.AllianceFlags == 0x00)
            {
                ImGui.TextUnformatted(" - No Alliance - ");
                return;
            }

            ImGui.TextUnformatted("Alliance Members:");
            using var table = ImRaii.Table($"Alliance#{index}", 4);
            if (!table.Success)
                return;

            Helper.TableHeader("Index", "Name", "CurrentHP", "ClassJob");
            for (var i = 0; i < 20; i++)
            {
                var allianceMember = groupManager->MainGroup.GetAllianceMemberByIndex(i);
                if (IsPlayerInitialized(allianceMember))
                {
                    try
                    {
                        DebugPartyMemberRow(allianceMember, i);
                    }
                    catch (Exception ex)
                    {
                        DutyTracker.Log.Error(ex, "Error occured.");
                        Helper.TableRow($"{i}", "error", "error", "error");
                    }
                }
                else
                {
                    Helper.TableRow($"{i}", "-", "-", "-");
                }
            }
        }
    }

    private static void DebugPartyMemberRow(PartyMember* partyMember, int i)
    {
        Helper.TableRow($"{i}", $"{partyMember->NameString}", $"{partyMember->CurrentHP}", $"{partyMember->ClassJob}");
    }

    private static void EmptyPlayerRow(int i)
    {
        Helper.TableRow($"{i}", "-", "-", "-");
    }

    public void DebugCache()
    {
        ImGui.TextUnformatted($"Party Alliance = {PartyAlliance}");
        ImGui.TextUnformatted($"Alliance1 = {Alliance1}");
        ImGui.TextUnformatted($"Alliance2 = {Alliance2}");
        ImGui.TextUnformatted($"Alliance3 = {Alliance3}");
        ImGui.TextUnformatted($"Alliance4 = {Alliance4}");
        ImGui.TextUnformatted($"Alliance5 = {Alliance5}");

        ImGui.TextUnformatted($"Party State = {PartyState}");
        ImGui.TextUnformatted($"Alliance State = {AllianceState}");

        ImGui.TextUnformatted("Party Cache");
        using var table = ImRaii.Table("PartyCache", 4);
        {
            if (table.Success)
            {
                Helper.TableHeader("", "Name", "HP", "Alliance");
                for (var i = 0; i < PartyCache.Length; i++)
                {
                    var cachedMember = PartyCache[i];
                    if (cachedMember is not null)
                        Helper.TableRow($"{i}", $"{cachedMember.Name}", $"{cachedMember.Hp}", $"{cachedMember.Alliance}");
                    else
                        EmptyPlayerRow(i);
                }
            }
        }

        ImGui.TextUnformatted("Alliance Cache");
        using var lowerTable = ImRaii.Table("AllianceCache", 4);
        {
            if (lowerTable.Success)
            {
                Helper.TableHeader("", "Name", "HP", "Alliance");
                for (var i = 0; i < AllianceCache.Length; i++)
                {
                    var cachedMember = AllianceCache[i];
                    if (cachedMember is not null)
                        Helper.TableRow($"{i}", $"{cachedMember.Name}", $"{cachedMember.Hp}", $"{cachedMember.Alliance}");
                    else
                        EmptyPlayerRow(i);
                }
            }
        }
    }

    #endregion
}