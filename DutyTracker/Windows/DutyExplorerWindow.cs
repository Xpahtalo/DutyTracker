using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using DutyTracker.DutyEvents;
using DutyTracker.Extensions;
using ImGuiNET;

namespace DutyTracker.Windows;

public class DutyExplorerWindow : Window, IDisposable
{
    private readonly DutyTracker DutyTracker;

    private Duty? SelectedDuty;
    private Run? SelectedRun;

    public DutyExplorerWindow(DutyTracker dutyTracker) : base("Duty Explorer")
    {
        Flags = ImGuiWindowFlags.AlwaysAutoResize;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(820, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        DutyTracker = dutyTracker;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var availableRegion = ImGui.GetContentRegionAvail();

        DrawDutyList(new Vector2(availableRegion.X * 0.25f, 0));
        ImGui.SameLine();
        DrawRunList(new Vector2(availableRegion.X * 0.25f, 0), SelectedDuty);
        ImGui.SameLine();

        // ImGui will automatically fill the rest of the space if passed a width of zero.
        // (availableRegion.X * 0.25f) resulted in the right edge of the child window being clipped.
        DrawRunInfo(new Vector2(availableRegion.X * 0, 0), SelectedRun);
    }

    private void DrawDutyList(Vector2 size)
    {
        using var child = ImRaii.Child("##Duties", size, true);
        if (!child.Success)
            return;

        if (DutyTracker.DutyManager.AnyDutiesStarted)
        {
            var listWidth = ImGui.GetContentRegionAvail().X;
            var listLength = (ImGui.GetContentRegionAvail().Y / ImGui.GetTextLineHeightWithSpacing()) * ImGui.GetTextLineHeightWithSpacing();
            using var listBox = ImRaii.ListBox("##DutyList", new Vector2(listWidth, listLength));
            if (!listBox.Success)
                return;

            foreach (var (duty, idx) in DutyTracker.DutyManager.DutyList.Select((val, i) => (val, i)))
            {
                if (!ImGui.Selectable($"{duty.TerritoryType.PlaceName.Value?.Name ?? "Report This"}##{idx}", SelectedDuty == duty))
                    continue;

                SelectedRun = null;
                SelectedDuty = SelectedDuty == duty ? null : duty;
            }
        }
        else
        {
            ImGui.TextUnformatted("No duties to display.");
        }
    }

    private void DrawRunList(Vector2 size, Duty? duty)
    {
        using var child = ImRaii.Child("##RunList", size, true);
        if (!child.Success)
            return;

        if (duty is not null)
        {
            var infoWidth = ImGui.CalcTextSize("###########################").X;
            var averageDeaths = duty.TotalWipes == 0 ? 0 : duty.TotalDeaths / duty.TotalWipes;
            Helper.InfoText("Duty Start Time:", $@"{duty.StartTime:hh\:mm\:ss tt}", infoWidth);
            Helper.InfoText("Duty End Time:", $@"{duty.EndTime:hh\:mm\:ss tt}", infoWidth);
            Helper.InfoText("Duty Duration:", $"{duty.Duration.HoursMinutesAndSeconds()}", infoWidth);
            Helper.InfoText("Wipes:", $"{duty.TotalWipes}", infoWidth);
            Helper.InfoText("Deaths:", $"{duty.TotalDeaths}", infoWidth);
            Helper.InfoText("Average Deaths:", $"{averageDeaths}", infoWidth);

            var listWidth = ImGui.GetContentRegionAvail().X;
            var listLength = (ImGui.GetContentRegionAvail().Y / ImGui.GetTextLineHeightWithSpacing()) * ImGui.GetTextLineHeightWithSpacing();
            using var listBox = ImRaii.ListBox("##RunList", new Vector2(listWidth, listLength));
            if (!listBox.Success)
                return;

            foreach (var (run, idx) in duty.RunList.Select((val, i) => (val, i)))
                if (ImGui.Selectable($@"{idx + 1} - {run.StartTime:hh\:mm\:ss tt}", SelectedRun == run))
                    SelectedRun = SelectedRun == run ? null : run;
        }
        else
        {
            ImGui.TextUnformatted("Please select a duty.");
        }
    }

    private void DrawRunInfo(Vector2 size, Run? run)
    {
        using var child = ImRaii.Child("##RunInfo", size, true);
        if (!child.Success)
            return;

        if (run is not null)
        {
            var infoWidth = ImGui.CalcTextSize("###########################").X;
            Helper.InfoText("Run Start Time:", $@"{run.StartTime:hh\:mm\:ss tt}", infoWidth);
            Helper.InfoText("Run End Time:", $@"{run.EndTime:hh\:mm\:ss tt}", infoWidth);
            Helper.InfoText("Run Duration:", $"{run.Duration.HoursMinutesAndSeconds()}", infoWidth);
            Helper.InfoText("Deaths:", $"{run.DeathList.Count}", infoWidth);

            if (run.DeathList.Count <= 0)
                return;

            using var table = ImRaii.Table("Deaths", 2, ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.RowBg);
            if (!table.Success)
                return;

            Helper.TableHeader("Player Names", "Time of Death", "Alliance");
            foreach (var death in run.DeathList)
                Helper.TableRow(death.PlayerName, $@"{death.TimeOfDeath:hh\:mm\:ss tt}", $"{death.Alliance}");
        }
        else
        {
            ImGui.TextUnformatted("Please select a run.");
        }
    }
}