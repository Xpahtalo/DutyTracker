using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using Dalamud.Game;
using DutyTracker.Enums;
using ImGuiNET;
using Framework = Dalamud.Game.Framework;

namespace DutyTracker.Services;

public class CachedPartyMember
{
    public string   Name     { get; }
    public uint     ObjectId { get; }
    public uint     Hp       { get; set; }
    public Alliance Alliance { get; }

    public CachedPartyMember(string name, uint objectId, uint hp, Alliance alliance)
    {
        Name     = name;
        ObjectId = objectId;
        Hp       = hp;
        Alliance = alliance;
    }

    public override string ToString()
    {
        return $"Name: {Name}, ObjectId: {ObjectId}, HP: {Hp}, Alliance: {Alliance}";
    }
}

internal enum State
{
    NoGroup,
    WaitingForData,
    Running,
}

public unsafe class PlayerCharacterState : IDisposable
{
    private readonly int allianceNumberArray     = 68;
    private readonly int allianceNumberArraySize = 296;
    private readonly int allianceStringArray     = 63;
    private readonly int allianceStringArraySize = 45;

    private const int Party1Position = 0;
    private const int Party2Position = 9;
    private const int Party3Position = 18;
    private const int Party4Position = 27;
    private const int Party5Position = 36;
    private const int AllianceSize = 40;
    private const int PartySize    = 8;


    private readonly CachedPartyMember?[] allianceCache;

    private State state;

    private AllianceType allianceType;
    private Alliance     partyAlliance;
    private Alliance     alliance1;
    private Alliance     alliance2;
    private Alliance     alliance3;
    private Alliance     alliance4;
    private Alliance     alliance5;

    public delegate void OnPlayerDeathDelegate(string PlayerName, uint ObjectId, Alliance alliance);

    public event OnPlayerDeathDelegate? OnPlayerDeath;

    public PlayerCharacterState()
    {
        state                    =  State.NoGroup;
        allianceCache            =  new CachedPartyMember?[AllianceSize];
        Service.Framework.Update += FrameworkUpdate;
        PluginLog.Verbose("PlayerCharacterState initialized.");
    }

    public void Dispose()
    {
        Service.Framework.Update -= FrameworkUpdate;
    }

    public void FrameworkUpdate(Framework framework)
    {
        var groupManager  = GroupManager.Instance();
        var allianceFlags = groupManager->AllianceFlags;

        switch (state)
        {
            case State.NoGroup:
                if (allianceFlags != 0)
                {
                    state = State.WaitingForData;
                    PluginLog.Verbose($"Changing state to WaitingForData.");
                }


                break;
            case State.WaitingForData:
                // Need the alliance string array to be populated to determine which parties are which alliance,
                // but it is not populated until the game is ready to draw the alliance list.
                if (IsAllianceStringDataPopulated())
                {
                    SetAlliances(groupManager);
                    state = State.Running;
                    PluginLog.Debug($"Alliance data detected. Changing state to Running.");
                }

                break;
            case State.Running:
                if (allianceFlags == 0)
                {
                    state = State.NoGroup;
                    ClearAllianceCache();
                    PluginLog.Debug("Alliance no longer detected. Changing state to NoGroup.");
                    break;
                }

                UpdatePlayerCache(groupManager);
                break;
        }
    }

    private void UpdatePlayerCache(GroupManager* groupManager)
    {
        for (var i = 0; i < AllianceSize; i++)
        {
            var allianceMember = TryGetAllianceMemberByIndex(out var playerExists);

            if (allianceCache[i] is not null)
            {
                // Have to check if the player exists first, because the health check will be true if they don't.
                // This would result in player being marked dead instead of leaving.
                if (!playerExists)
                {
                    PlayerLeft(i);
                }
                else
                {
                    if (allianceCache[i]!.Hp != allianceMember->CurrentHP)
                    {
                        allianceCache[i]!.Hp = allianceMember->CurrentHP;

                        if (allianceCache[i]!.Hp == 0)
                        {
                            PlayerDied(i);
                        }
                    }
                }
            }
            else
            {
                if (playerExists)
                {
                    AddPlayer(allianceMember, i);
                }
            }

            PartyMember* TryGetAllianceMemberByIndex(out bool found)
            {
                int group;
                int index;

                // Alliance size and count changes depending on the alliance type.
                // ThreeParty is 2 other parties of 8 members.
                // SixParty is 5 other parties of 4 types.
                if (allianceType == AllianceType.ThreeParty)
                {
                    group = i % 2;
                    index = i % 8;
                }
                else
                {
                    group = i % 5;
                    index = i % 4;
                }

                PluginLog.Debug($"{i}, {group}, {index}");
                try
                {
                    var member = groupManager->GetAllianceMemberByGroupAndIndex(group, index);
                    if (member->ObjectID != nint.Zero)
                    {
                        found = true;
                        return member;
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.Warning(ex.ToString());
                }

                found = false; 
                return null;
            }
        }
    }

    private void AddPlayer(PartyMember* allianceMember, int i)
    {
        var newPlayer = new CachedPartyMember(Marshal.PtrToStringUTF8(new IntPtr(allianceMember->Name)), allianceMember->ObjectID, allianceMember->CurrentHP, GetAlliance(i));
        PluginLog.Debug($"Detected new player: {i}, {newPlayer}");
        allianceCache[i] = newPlayer;
    }

    private void PlayerDied(int i)
    {
        var playerName = allianceCache[i].Name;
        var objectId   = allianceCache[i].ObjectId;
        var alliance   = allianceCache[i].Alliance;

        PluginLog.Debug($"Alliance member died: {i}, {playerName}, {objectId}, {alliance}");
        OnPlayerDeath?.Invoke(playerName,
                              objectId,
                              alliance);
    }


    private void PlayerLeft(int i)
    {
        PluginLog.Debug($"Player left alliance. Removing {allianceCache[i].Name}");
        allianceCache[i] = null;
    }

    private bool IsAllianceStringDataPopulated()
    {
        var stringArray =
            FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetRaptureAtkModule()
                ->AtkModule.AtkArrayDataHolder.StringArrays[63]->StringArray;

        return !string.IsNullOrWhiteSpace(Marshal.PtrToStringUTF8(new IntPtr(stringArray[0])));
    }

    private Alliance GetAlliance(int i) =>
        i switch
        {
            < Party1Position => alliance1,
            < Party2Position => alliance2,
            < Party3Position => alliance3,
            < Party4Position => alliance4,
            < Party5Position => alliance5,
            _                => Alliance.None,
        };

    private void SetAlliances(GroupManager* groupManager)
    {
        var stringArray =
            FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetRaptureAtkModule()
                ->AtkModule.AtkArrayDataHolder.StringArrays[63]->StringArray;
        PluginLog.Debug("Three Party Strings");
        PluginLog.Debug($"  string{Party1Position}:  {Marshal.PtrToStringUTF8(new IntPtr(stringArray[Party1Position]))}");
        PluginLog.Debug($"  string{Party2Position}:  {Marshal.PtrToStringUTF8(new IntPtr(stringArray[Party2Position]))}");
        PluginLog.Debug($"  string{Party3Position}: {Marshal.PtrToStringUTF8(new IntPtr(stringArray[Party3Position]))}");
        PluginLog.Debug($"  string{Party4Position}: {Marshal.PtrToStringUTF8(new IntPtr(stringArray[Party4Position]))}");
        PluginLog.Debug($"  string{Party5Position}: {Marshal.PtrToStringUTF8(new IntPtr(stringArray[Party5Position]))}");
        alliance1 = AllianceExtensions.ToAlliance(stringArray[Party1Position]);
        alliance2 = AllianceExtensions.ToAlliance(stringArray[Party2Position]);
        alliance3 = AllianceExtensions.ToAlliance(stringArray[Party3Position]);
        alliance4 = AllianceExtensions.ToAlliance(stringArray[Party4Position]);
        alliance5 = AllianceExtensions.ToAlliance(stringArray[Party5Position]);
        
        if (groupManager->AllianceFlags == 1)
        {
            allianceType = AllianceType.ThreeParty;
            partyAlliance = (alliance1, alliance2) switch
            {
                (Alliance.A, Alliance.B) => Alliance.C,
                (Alliance.A, Alliance.C) => Alliance.B,
                (Alliance.B, Alliance.C) => Alliance.A,
                _                        => Alliance.None,
            };
        }
        else
        {
            allianceType = AllianceType.SixParty;
            partyAlliance = (alliance1, alliance2, alliance3, alliance4, alliance5) switch
            {
                (Alliance.A, Alliance.B, Alliance.C, Alliance.D, Alliance.E) => Alliance.F,
                (Alliance.A, Alliance.B, Alliance.C, Alliance.D, Alliance.F) => Alliance.E,
                (Alliance.A, Alliance.B, Alliance.C, Alliance.E, Alliance.F) => Alliance.D,
                (Alliance.A, Alliance.B, Alliance.D, Alliance.E, Alliance.F) => Alliance.C,
                (Alliance.A, Alliance.C, Alliance.D, Alliance.E, Alliance.F) => Alliance.B,
                (Alliance.B, Alliance.C, Alliance.D, Alliance.E, Alliance.F) => Alliance.A,
                _                                                            => Alliance.None,
            };
        }


        PluginLog.Verbose($"Alliance Type: {allianceType}");
        PluginLog.Verbose($"Party Alliance: {partyAlliance}");
        PluginLog.Verbose($"Alliance Parties: {alliance1}, {alliance2}, {alliance3}, {alliance4}, {alliance5}");
    }

    private void ClearAllianceCache()
    {
        for (var i = 0; i < AllianceSize; i++)
        {
            allianceCache[i] = null;
        }
    }

    #region DebugInfo

    public void ImGuiParty()
    {
        var groupManager1 = GroupManager.Instance();

        ImGui.Text($"Alliance Flags: {groupManager1->AllianceFlags}");

        if (groupManager1->MemberCount == nint.Zero)
        {
            ImGui.Text("No Group Manager Available.");
            return;
        }
        else
        {

            ImGui.Text("Party");
            DebugParty(groupManager1);
        }

        if (groupManager1->AllianceFlags != 0x00)
        {
            ImGui.Text("Alliance");
            DebugAlliance(groupManager1);
        }
    }

    private void DebugParty(GroupManager* groupManager)
    {
        if (ImGui.BeginTable("Party", 5))
        {
            XGui.TableHeader("Index", "Name", "ObjectID", "CurrentHP", "ClassJob");

            var partyMembers = (PartyMember*)groupManager->PartyMembers;
            for (var i = 0; i < groupManager->MemberCount; i++)
            {
                DebugPartyMemberRow(partyMembers[i], i);
            }
        }

        ImGui.EndTable();
    }

    private void DebugPartyMemberRow(PartyMember partyMember, int i)
    {
        XGui.TableRow($"{i}",
                      $"{Marshal.PtrToStringUTF8(new IntPtr(partyMember.Name))}",
                      $"{partyMember.ObjectID}",
                      $"{partyMember.CurrentHP}",
                      $"{partyMember.ClassJob}");
    }

    public void DebugAlliance(GroupManager* groupManager)
    {
        if (ImGui.BeginTable("Alliance", 5))
        {
            XGui.TableHeader("Index", "Name", "ObjectID", "CurrentHP", "ClassJob");
            var allianceMembers = (PartyMember*)groupManager->AllianceMembers;
            for (var i = 0; i < AllianceSize; i++)
            {
                DebugPartyMemberRow(allianceMembers[i], i);
            }
        }

        ImGui.EndTable();
    }

    public void DebugCache()
    {
        ImGui.Text("Cache");

        for(var i =0;i<allianceCache.Length;i++)
        {
            ImGui.Text($"{i}");
            if (allianceCache[i] is not null)
            {
                ImGui.SameLine();
                ImGui.Text($"{allianceCache[i]}");
            }
        }
    }

    #endregion
}
