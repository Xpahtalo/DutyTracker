using System;
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

        var newestDuty = dutyManager.GetMostRecentDuty();
        var newestRun  = dutyManager.GetMostRecentRun();
        
        if (newestDuty is not null)
        {
            XGui.InfoText($"Start Time:",           $"{newestDuty.StartTime:hh\\:mm\\:ss tt}");
            XGui.InfoText($"Start of Current Run:", $"{newestDuty.StartTime:hh\\:mm\\:ss tt}");
            if (dutyManager.DutyActive)
            {
                XGui.InfoText($"Elapsed Time:",     $"{newestDuty.Duration.MinutesAndSeconds()}");
                if (newestRun is not null)
                    XGui.InfoText($"Current Run Time:", $"{newestRun.Duration.MinutesAndSeconds()}");
            }
            else
            {
                XGui.InfoText($"Final Run Time:",  $"{newestDuty.Duration.MinutesAndSeconds()}");
                if (newestRun is not null)
                    XGui.InfoText($"Total Duty Time:", $"{newestRun.Duration.MinutesAndSeconds()}");
            }

            XGui.InfoText($"In Duty:",         $"{dutyManager.DutyActive}");
            XGui.InfoText($"Party Deaths:", $"{newestDuty.TotalDeaths}");
            XGui.InfoText($"Wipes:",           $"{newestDuty.TotalWipes}");
        }
        else
        {
            // This only happens if no duties have been started since the plugin loaded.
            ImGui.Text("No duties");
        }


        if (ImGui.Button("Open Explorer"))
        {
            windowSystem.GetWindow("Duty Explorer")!.IsOpen = true;
        }

        if (ImGui.Button("Open Debug"))
        {
            windowSystem.GetWindow("Debug")!.IsOpen = true;
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
