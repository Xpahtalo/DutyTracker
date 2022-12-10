using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DutyTracker.Duty_Events;
using DutyTracker.Extensions;
using Lumina.Excel;
using ImGuiNET;
using Lumina.Data;
using Lumina.Excel.GeneratedSheets;

namespace DutyTracker.Windows;

public class DutyExplorerWindow : Window, IDisposable
{
    private DutyManager   dutyManager;
    private Configuration configuration;
    private Duty?         selectedDuty;
    private Run?          selectedRun;

    private static ImGuiTableFlags TableFlags = ImGuiTableFlags.BordersV      |
                                                ImGuiTableFlags.BordersOuterH |
                                                ImGuiTableFlags.RowBg;

    public DutyExplorerWindow(DutyManager dutyManager, Configuration configuration)
        : base("Duty Explorer")
    {
        SizeConstraints = new WindowSizeConstraints
                          {
                              MinimumSize = new Vector2(375,            330),
                              MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
                          };
        
        this.dutyManager   = dutyManager;
        this.configuration = configuration;
        
    }

    public          void Dispose() { }
    
    public override void Draw()
    {
        var availableRegion = ImGui.GetContentRegionAvail();
        
        DrawDutyList(new Vector2(availableRegion.X * 0.25f, 0));
        ImGui.SameLine();
        DrawRunList(new Vector2(availableRegion.X * 0.25f, 0), selectedDuty);
        ImGui.SameLine();
        DrawRunInfo(new Vector2(availableRegion.X * 0.49f, 0), selectedRun);
    }

    private void DrawDutyList(Vector2 size)
    {
        var index = 0;
        if (ImGui.BeginChild("##Duties", size, true))
        {
            if (dutyManager.AnyDutiesStarted)
            {
                if (ImGui.BeginListBox("##DutyList"))
                {
                    foreach (var duty in dutyManager.Duties)
                    {
                        if (ImGui.Selectable($"{Service.DataManager.Excel.GetSheet<TerritoryType>()!.GetRow(duty.TerritoryType)!.PlaceName.Value!.Name}##{index}", selectedDuty == duty))
                        {
                            selectedDuty = selectedDuty == duty ? null : duty;
                            selectedRun  = null;
                        }

                        index++;
                    }
                }

                ImGui.EndListBox();
            }
            else
            {
                ImGui.Text("No duties to display.");
            }
        }

        ImGui.EndChild();
    }

    private void DrawRunList(Vector2 size, Duty? duty)
    {
        if (ImGui.BeginChild("##RunList", size, true))
        {
            if (duty is not null)
            {
                ImGui.Text($"Duty Start Time: {duty.StartTime:hh\\:mm\\:ss tt}");
                ImGui.Text($"Duty End Time:   {duty.EndTime:hh\\:mm\\:ss tt}");
                ImGui.Text($"Duty Duration:   {(duty.EndTime - duty.StartTime).HoursMinutesAndSeconds()}");
                ImGui.Text($"Wipes:  {duty.TotalWipes}");
                ImGui.Text($"Deaths: {duty.TotalDeaths}");
                var averageDeaths = duty.TotalWipes == 0 ? 0 : duty.TotalDeaths / duty.TotalWipes;
                ImGui.Text($"Average Deaths: {averageDeaths}");

                if (ImGui.BeginListBox("##RunList"))
                {
                    foreach (var run in duty.RunList)
                    {
                        if (ImGui.Selectable($"{run.StartTime:hh\\:mm\\:ss tt}", selectedRun == run))
                        {
                            selectedRun = selectedRun == run ? null : run;
                        }
                    }
                }
                ImGui.EndListBox();
            }
            else
            {
                ImGui.Text("Please select a duty.");
            }
        }
        ImGui.EndChild();
    }

    private void DrawRunInfo(Vector2 size, Run? run)
    {
        if (ImGui.BeginChild("##RunInfo", size, true))
        {
            if (run is not null)
            {
                ImGui.Text($"Run Start Time: {run.StartTime:hh\\:mm\\:ss tt}");
                ImGui.Text($"Run End Time:   {run.EndTime:hh\\:mm\\:ss tt}");
                ImGui.Text($"Run Duration:   {(run.EndTime - run.StartTime).HoursMinutesAndSeconds()}");
                ImGui.Text($"Deaths: {run.DeathList.Count}");
                
                if (run.DeathList.Count > 0)
                {
                    if (ImGui.BeginTable("deaths", 2, TableFlags))
                    {
                        ImGui.TableSetupColumn("Player Name");
                        ImGui.TableSetupColumn("Time of Death");
                        ImGui.TableHeadersRow();
                

                        foreach (var death in run.DeathList)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.TextUnformatted($"{death.PlayerName}");
                            ImGui.TableSetColumnIndex(1);
                            ImGui.TextUnformatted($"{death.TimeOfDeath:hh\\:mm\\:ss tt}");
                        }
                    }

                    ImGui.EndTable();
                }
            }
            else
            {
                ImGui.Text("Please select a run.");
            }
        }
        ImGui.EndChild();
    }
}
