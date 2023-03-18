using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DutyTracker.Duty_Events;
using DutyTracker.Extensions;
using ImGuiNET;

namespace DutyTracker.Windows;

public class DutyExplorerWindow : Window, IDisposable
{
    private readonly DutyManager _dutyManager;
    private          Duty?       _selectedDuty;
    private          Run?        _selectedRun;

    // This value is just chosen to look good.
    private static readonly float InfoWidth = ImGui.CalcTextSize("###########################").X;

    private const ImGuiTableFlags DeathsTableFlags = ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.RowBg;

    public DutyExplorerWindow(DutyManager dutyManager)
        : base("Duty Explorer")
    {
        SizeConstraints = new WindowSizeConstraints
                          {
                              MinimumSize = new Vector2(820,            330),
                              MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
                          };

        this._dutyManager = dutyManager;

    }

    public void Dispose() { }

    public override void Draw()
    {
        var availableRegion = ImGui.GetContentRegionAvail();

        DrawDutyList(new Vector2(availableRegion.X * 0.25f, 0));
        ImGui.SameLine();
        DrawRunList(new Vector2(availableRegion.X * 0.25f, 0), _selectedDuty);
        ImGui.SameLine();
        // ImGui will automatically fill the rest of the space if passed a width of zero.
        // (availableRegion.X * 0.25f) resulted in the right edge of the child window being clipped.
        DrawRunInfo(new Vector2(availableRegion.X * 0, 0), _selectedRun);
    }

    private void DrawDutyList(Vector2 size)
    {
        var index = 0;
        if (ImGui.BeginChild("##Duties", size, true)) {
            if (_dutyManager.AnyDutiesStarted) {
                var listWidth  = ImGui.GetContentRegionAvail().X;
                var listLength = (ImGui.GetContentRegionAvail().Y / ImGui.GetTextLineHeightWithSpacing()) * ImGui.GetTextLineHeightWithSpacing();

                if (ImGui.BeginListBox("##DutyList", new Vector2(listWidth, listLength))) {
                    foreach (var duty in _dutyManager.DutyList) {
                        if (ImGui.Selectable($"{duty.TerritoryType.PlaceName.Value?.Name ?? "Report This"}##{index}", _selectedDuty == duty)) {
                            _selectedDuty = _selectedDuty == duty ? null : duty;
                            _selectedRun  = null;
                        }

                        index++;
                    }
                }

                ImGui.EndListBox();
            } else {
                ImGui.Text("No duties to display.");
            }
        }

        ImGui.EndChild();
    }

    private void DrawRunList(Vector2 size, Duty? duty)
    {
        if (ImGui.BeginChild("##RunList", size, true)) {
            if (duty is not null) {
                var averageDeaths = duty.TotalWipes == 0 ? 0 : duty.TotalDeaths / duty.TotalWipes;
                XGui.InfoText("Duty Start Time:", $"{duty.StartTime:hh\\:mm\\:ss tt}",         InfoWidth);
                XGui.InfoText($"Duty End Time:",  $"{duty.EndTime:hh\\:mm\\:ss tt}",           InfoWidth);
                XGui.InfoText($"Duty Duration:",  $"{duty.Duration.HoursMinutesAndSeconds()}", InfoWidth);
                XGui.InfoText($"Wipes:",          $"{duty.TotalWipes}",                        InfoWidth);
                XGui.InfoText($"Deaths:",         $"{duty.TotalDeaths}",                       InfoWidth);
                XGui.InfoText($"Average Deaths:", $"{averageDeaths}",                          InfoWidth);

                var listWidth  = ImGui.GetContentRegionAvail().X;
                var listLength = (ImGui.GetContentRegionAvail().Y / ImGui.GetTextLineHeightWithSpacing()) * ImGui.GetTextLineHeightWithSpacing();

                if (ImGui.BeginListBox("##RunList", new Vector2(listWidth, listLength))) {
                    var index = 1;
                    foreach (var run in duty.RunList) {
                        if (ImGui.Selectable($"{index++} - {run.StartTime:hh\\:mm\\:ss tt}", _selectedRun == run)) {
                            _selectedRun = _selectedRun == run ? null : run;
                        }
                    }
                }

                ImGui.EndListBox();
            } else {
                ImGui.Text("Please select a duty.");
            }
        }

        ImGui.EndChild();
    }

    private void DrawRunInfo(Vector2 size, Run? run)
    {
        if (ImGui.BeginChild("##RunInfo", size, true)) {
            if (run is not null) {
                XGui.InfoText("Run Start Time:", $"{run.StartTime:hh\\:mm\\:ss tt}",         InfoWidth);
                XGui.InfoText("Run End Time:",   $"{run.EndTime:hh\\:mm\\:ss tt}",           InfoWidth);
                XGui.InfoText("Run Duration:",   $"{run.Duration.HoursMinutesAndSeconds()}", InfoWidth);
                XGui.InfoText("Deaths:",         $"{run.DeathList.Count}",                   InfoWidth);

                if (run.DeathList.Count > 0) {
                    if (ImGui.BeginTable("deaths", 2, DeathsTableFlags)) {
                        XGui.TableHeader("Player Names", "Time of Death", "Alliance");

                        foreach (var death in run.DeathList) {
                            XGui.TableRow($"{death.PlayerName}", $"{death.TimeOfDeath:hh\\:mm\\:ss tt}", $"{death.Alliance}");
                        }
                    }

                    ImGui.EndTable();
                }
            } else {
                ImGui.Text("Please select a run.");
            }
        }

        ImGui.EndChild();
    }
}
