using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DutyTracker.Duty_Events;
using DutyTracker.Extensions;
using ImGuiNET;

namespace DutyTracker.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private DutyManager   dutyManager;
    private Configuration configuration;

    private static ImGuiTableFlags TableFlags = ImGuiTableFlags.BordersV | 
                                                ImGuiTableFlags.BordersOuterH | 
                                                ImGuiTableFlags.RowBg;

    public MainWindow(DutyManager dutyManager, Configuration configuration) : base(
        "Duty Tracker")
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        this.dutyManager   = dutyManager;
        this.configuration = configuration;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        if(ImGui.BeginTabBar("MainWindowTabBar"))
        {
            DisplayStatusTab();
            DisplayOptionsTab();
            DisplayInfoTab();
        }
        ImGui.EndTabBar();
    }

    private void DisplayStatusTab()
    {
        if (!ImGui.BeginTabItem("Status"))
            return;

        ImGui.Text($"Start Time: {dutyManager.Duty.StartOfDuty:hh\\:mm\\:ss tt}");
        ImGui.Text($"Start of Current Run: {dutyManager.Duty.StartOfCurrentRun:hh\\:mm\\:ss tt}");
        ImGui.Text($"End Time: {dutyManager.Duty.EndOfDuty:hh\\:mm\\:ss tt}");
        ImGui.Text($"Elapsed Time: {dutyManager.TotalDutyTime.MinutesAndSeconds()}");
        ImGui.Text($"Current Run Time: {dutyManager.CurrentRunTime.MinutesAndSeconds()}");
        ImGui.Text($"Duty Status: {dutyManager.DutyActive}");
        ImGui.Text($"Party Deaths: {dutyManager.Duty.DeathEvents.Count}");
        ImGui.Text($"Wipes: {dutyManager.Duty.WipeEvents.Count}");
        
        if (dutyManager.Duty.DeathEvents.Count > 0)
        {
            if (ImGui.BeginTable("deaths", 2, TableFlags))
            {
                ImGui.TableSetupColumn("Player Name");
                ImGui.TableSetupColumn("Time of Death");
                ImGui.TableHeadersRow();
                
                foreach (var deathEvent in dutyManager.Duty.DeathEvents)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextUnformatted($"{deathEvent.PlayerName}");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.TextUnformatted($"{deathEvent.TimeOfDeath:hh\\:mm\\:ss tt}");
                }
            }
            ImGui.EndTable();
        }

        if (dutyManager.Duty.WipeEvents.Count > 0)
        {
            if (ImGui.BeginTable("wipes", 2, TableFlags))
            {
                ImGui.TableSetupColumn("Run Duration");
                ImGui.TableSetupColumn("Time of Wipe");

                ImGui.TableHeadersRow();

                foreach (var wipeEvent in dutyManager.Duty.WipeEvents)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextUnformatted($"{wipeEvent.Duration.MinutesAndSeconds()}");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.TextUnformatted($"{wipeEvent.TimeOfWipe:hh\\:mm\\:ss tt}");
                }
            }
            ImGui.EndTable();
        }
        

        ImGui.EndTabItem();
    }

    private void DisplayOptionsTab()
    {
        if (!ImGui.BeginTabItem("Options"))
            return;

        var includeDutyTrackerLabel = configuration.IncludeDutyTrackerLabel;
        if (ImGui.Checkbox("Include [DutyTracker] label", ref includeDutyTrackerLabel))
            configuration.IncludeDutyTrackerLabel = includeDutyTrackerLabel;
        
        var suppressEmptyValues = configuration.SuppressEmptyValues;
        if (ImGui.Checkbox("Suppress values that are zero", ref suppressEmptyValues))
            configuration.SuppressEmptyValues = suppressEmptyValues;
        
        if(ImGui.Button("Save"))
            configuration.Save();
        
        ImGui.EndTabItem();
    }

    private void DisplayInfoTab()
    {
        if (!ImGui.BeginTabItem("Info"))
            return;
        
        ImGui.TextWrapped("Currently, only the deaths of party members are tracked. This means that deaths that occur in other alliances will not be shown.");
        ImGui.TextWrapped("Party members must also be loaded in order to be tracked, which should handle almost all cases, but there may be some that are out of my control.");
        
        ImGui.EndTabItem();
    }
}
