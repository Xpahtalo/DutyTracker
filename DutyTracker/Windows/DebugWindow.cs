using System;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using DutyTracker.Extensions;
using DutyTracker.Services.PlayerCharacter;
using ImGuiNET;

namespace DutyTracker.Windows;

public class DebugWindow : Window, IDisposable
{
    public DebugWindow() : base("Debug") { }

    public void Dispose() { }

    public override void Draw()
    {
        using var tabBar = ImRaii.TabBar("DebugTabBar");
        if (!tabBar.Success)
            return;

        DisplayFfxivDebug();
        DisplayPlayerCharacterStateDebug();
        DisplayTerritoryInfo();
    }

    private void DisplayFfxivDebug()
    {
        using var tabItem = ImRaii.TabItem("GroupManager");
        if (!tabItem.Success)
            return;

        PlayerCharacterState.DebugGroupManager();
    }

    private void DisplayPlayerCharacterStateDebug()
    {
        using var tabItem = ImRaii.TabItem("PlayerCharacterState");
        if (!tabItem.Success)
            return;

        DutyTracker.PlayerCharacterState.DebugCache();
    }

    private void DisplayTerritoryInfo()
    {
        using var tabItem = ImRaii.TabItem("ClientState");
        if (!tabItem.Success)
            return;

        var territory = Sheets.TerritorySheet.GetRow(DutyTracker.ClientState.TerritoryType);
        if (territory is null)
        {
            ImGui.TextUnformatted("Null territory");
            return;
        }

        var intendedUse = territory.GetIntendedUseEnum();
        ImGui.TextUnformatted($"Territory Type: {territory}");
        ImGui.TextUnformatted($"Territory Name: {territory.PlaceName.Value?.Name} - {territory.Name}");
        ImGui.TextUnformatted($"IsPvpZone: {territory.IsPvpZone}");
        ImGui.TextUnformatted($"Intended use: {intendedUse} - {territory.TerritoryIntendedUse}");
        ImGui.TextUnformatted($"HasAlliance: {intendedUse.HasAlliance()}");
        ImGui.TextUnformatted($"IsRaidOrTrial: {intendedUse.IsRaidOrTrial()}");
        ImGui.TextUnformatted($"UsesBothGroupManagers: {intendedUse.UsesBothGroupManagers()}");
        ImGui.TextUnformatted($"ShouldTrack: {intendedUse.ShouldTrack()}");
    }
}