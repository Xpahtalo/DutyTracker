﻿using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DutyTracker.Duty_Events;
using DutyTracker.Extensions;
using ImGuiNET;

namespace DutyTracker.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private readonly DutyManager   dutyManager;
    private readonly Configuration configuration;
    private readonly WindowSystem  windowSystem;
    
    public MainWindow(DutyManager dutyManager, Configuration configuration, WindowSystem windowSystem) : base(
        "Duty Tracker")
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(248, 250),
            MaximumSize = new Vector2(248, 250),
        };

        Flags = ImGuiWindowFlags.NoResize;

        this.dutyManager   = dutyManager;
        this.configuration = configuration;
        this.windowSystem  = windowSystem;
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

        if (dutyManager.AnyDutiesStarted)
        {
            DutiesText();
        }
        else
        {
            NoDutiesText();
        }
        

        if (ImGui.Button("Open Explorer"))
        {
            windowSystem.GetWindow("Duty Explorer")!.IsOpen = true;
        }
        
        ImGui.EndTabItem();
    }

    private void NoDutiesText()
    {
        // This only happens if no duties have been started since the plugin loaded.
        ImGui.Text("No duties");
    }

    private void DutiesText()
    {
        var newestDuty         = dutyManager.Duties[^1];
        var newestDutyDuration = dutyManager.DutyActive ? DateTime.Now - newestDuty.StartTime : newestDuty.EndTime - newestDuty.StartTime;
        var newestRun          = newestDuty.RunList[^1];
        var newestRunDuration  = dutyManager.DutyActive ? DateTime.Now - newestRun.StartTime : newestRun.EndTime - newestRun.StartTime;

        XGui.InfoText($"Start Time:",           $"{newestDuty.StartTime:hh\\:mm\\:ss tt}");
        XGui.InfoText($"Start of Current Run:", $"{newestDuty.StartTime:hh\\:mm\\:ss tt}");
        if (dutyManager.DutyActive)
        {
            XGui.InfoText($"Elapsed Time:",     $"{newestDutyDuration.MinutesAndSeconds()}");
            XGui.InfoText($"Current Run Time:", $"{newestRunDuration.MinutesAndSeconds()}");
        }
        else
        {
            XGui.InfoText($"Final Run Time:",  $"{newestRunDuration.MinutesAndSeconds()}");
            XGui.InfoText($"Total Duty Time:", $"{newestDutyDuration.MinutesAndSeconds()}");
        }
        XGui.InfoText($"In Duty:",         $"{dutyManager.DutyActive}");
        XGui.InfoText($"Party DeathList:", $"{newestDuty.TotalDeaths}");
        XGui.InfoText($"Wipes:",           $"{newestDuty.TotalWipes}");
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
