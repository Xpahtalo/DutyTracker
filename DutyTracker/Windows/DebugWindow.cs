using System;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using DutyTracker.Enums;
using DutyTracker.Services;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace DutyTracker.Windows;

public class DebugWindow : Window, IDisposable
{
    private readonly IClientState          _clientState;
    private readonly IDataManager             _dataManager;
    private readonly PlayerCharacterState _playerCharacterState;

    public DebugWindow()
        : base("Debug")
    {
        _clientState          = Service.ClientState;
        _dataManager          = Service.DataManager;
        _playerCharacterState = Service.PlayerCharacterState;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("DebugTabBar")) {
            DisplayFfxivDebug();
            DisplayPlayerCharacterStateDebug();
            DisplayTerritoryInfo();
        }

        ImGui.EndTabBar();
    }

    private void DisplayFfxivDebug()
    {
        if (!ImGui.BeginTabItem("GroupManager"))
            return;

        PlayerCharacterState.DebugGroupManager();

        ImGui.EndTabItem();
    }

    private void DisplayPlayerCharacterStateDebug()
    {
        if (!ImGui.BeginTabItem("PlayerCharacterState"))
            return;

        _playerCharacterState.DebugCache();

        ImGui.EndTabItem();
    }

    private void DisplayTerritoryInfo()
    {
        if (!ImGui.BeginTabItem("ClientState"))
            return;

        var territoryRow = _dataManager.Excel.GetSheet<TerritoryType>();
        if (territoryRow is not null) {
            var territory = territoryRow.GetRow(_clientState.TerritoryType);

            if (territory is null) {
                ImGui.Text("Null territory");
                ImGui.EndTabItem();
                return;
            }

            var intendedUse = (TerritoryIntendedUse)territory.TerritoryIntendedUse;
            ImGui.Text($"Territory Type: {territory}");
            ImGui.Text($"Territory Name: {territory.PlaceName.Value?.Name} - {territory.Name}");
            ImGui.Text($"IsPvpZone: {territory.IsPvpZone}");
            ImGui.Text($"Intended use: {intendedUse} - {territory.TerritoryIntendedUse}");
            ImGui.Text($"HasAlliance: {intendedUse.HasAlliance()}");
            ImGui.Text($"IsRaidOrTrial: {intendedUse.IsRaidOrTrial()}");
            ImGui.Text($"UsesBothGroupManagers: {intendedUse.UsesBothGroupManagers()}");
            ImGui.Text($"ShouldTrack: {intendedUse.ShouldTrack()}");
        } else {
            ImGui.Text("Could not load sheet.");
        }

        ImGui.EndTabItem();
    }
}
