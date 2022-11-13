using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DutyTracker.Duty_Events;
using ImGuiNET;
using ImGuiScene;

namespace DutyTracker.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private DutyTracker dutyTracker;
    private DutyManager dutyManager;

    private static ImGuiTableFlags TableFlags = ImGuiTableFlags.BordersV | 
                                                ImGuiTableFlags.BordersOuterH | 
                                                ImGuiTableFlags.RowBg;

    public MainWindow(DutyTracker dutyTracker, DutyManager dutyManager) : base(
        "Duty Tracker", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        this.dutyTracker = dutyTracker;
        this.dutyManager = dutyManager;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        if(ImGui.BeginTabBar("MainWindowTabBar"))
        {
            DisplayStatus();
            DisplayOptions();
        }
        ImGui.EndTabBar();
    }

    private void DisplayStatus()
    {
        if (!ImGui.BeginTabItem("Status"))
            return;

        ImGui.Text($"Start Time: {dutyManager.Duty.StartOfDuty}");
        ImGui.Text($"Start of Current Run: {dutyManager.Duty.StartOfCurrentRun}");
        ImGui.Text($"End Time: {dutyManager.Duty.EndOfDuty}");
        ImGui.Text($"Elapsed Time: {dutyManager.TotalDutyTime:m\\:ss}");
        ImGui.Text($"Current Run Time: {dutyManager.CurrentRunTime:m\\:ss}");
        ImGui.Text($"Duty Status: {dutyManager.DutyActive}");
        ImGui.Text($"Deaths: {dutyManager.Duty.DeathEvents.Count}");
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
                    ImGui.TextUnformatted($"{deathEvent.TimeOfDeath}");
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
                    ImGui.TextUnformatted($"{wipeEvent.Duration:m\\:ss}");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.TextUnformatted($"{wipeEvent.TimeOfWipe}");
                }
            }
            ImGui.EndTable();
        }
        

        ImGui.EndTabItem();
    }

    private void DisplayOptions()
    {
        if (!ImGui.BeginTabItem("Options"))
            return;
        ImGui.EndTabItem();
    }
}
