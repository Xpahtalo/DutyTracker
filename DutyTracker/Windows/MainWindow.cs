using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DutyTracker.Duty_Events;
using DutyTracker.Extensions;
using ImGuiNET;

namespace DutyTracker.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private readonly DutyManager   _dutyManager;
    private readonly Configuration _configuration;
    
    public MainWindow(DutyManager dutyManager, Configuration configuration) : base(
        "Duty Tracker")
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(248, 250),
            MaximumSize = new Vector2(248, 250),
        };

        Flags = ImGuiWindowFlags.NoResize;

        _dutyManager   = dutyManager;
        _configuration = configuration;
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

        var newestDuty = _dutyManager.GetMostRecentDuty();
        var newestRun  = _dutyManager.GetMostRecentRun();

        if (newestDuty is not null) {
            XGui.InfoText($"Start Time:",           $"{newestDuty.StartTime:hh\\:mm\\:ss tt}");
            XGui.InfoText($"Start of Current Run:", $"{newestDuty.StartTime:hh\\:mm\\:ss tt}");
            if (_dutyManager.DutyActive) {
                XGui.InfoText($"Elapsed Time:", $"{newestDuty.Duration.MinutesAndSeconds()}");
                if (newestRun is not null)
                    XGui.InfoText($"Current Run Time:", $"{newestRun.Duration.MinutesAndSeconds()}");
            } else {
                XGui.InfoText($"Total Duty Time:", $"{newestDuty.Duration.MinutesAndSeconds()}");
                if (newestRun is not null)
                    XGui.InfoText($"Final Run Time:", $"{newestRun.Duration.MinutesAndSeconds()}");
            }

            XGui.InfoText($"In Duty:",      $"{_dutyManager.DutyActive}");
            XGui.InfoText($"Party Deaths:", $"{newestDuty.TotalDeaths}");
            XGui.InfoText($"Wipes:",        $"{newestDuty.TotalWipes}");
        } else {
            // This only happens if no duties have been started since the plugin loaded.
            ImGui.Text("No duties");
        }


        if (ImGui.Button("Open Explorer")) {
            Service.WindowService.ToggleWindow("DutyExplorer");
        }
#if DEBUG
        if (ImGui.Button("Open Debug")) {
            Service.WindowService.ToggleWindow("Debug");
        }
#endif
     
        ImGui.EndTabItem();
    }

    private void DisplayOptionsTab()
    {
        if (!ImGui.BeginTabItem("Options"))
            return;

        var includeDutyTrackerLabel = _configuration.IncludeDutyTrackerLabel;
        if (ImGui.Checkbox("Include [DutyTracker] label", ref includeDutyTrackerLabel))
            _configuration.IncludeDutyTrackerLabel = includeDutyTrackerLabel;
        
        var suppressEmptyValues = _configuration.SuppressEmptyValues;
        if (ImGui.Checkbox("Suppress values that are zero", ref suppressEmptyValues))
            _configuration.SuppressEmptyValues = suppressEmptyValues;
        
        if(ImGui.Button("Save"))
            _configuration.Save();
        
        ImGui.EndTabItem();
    }

    private void DisplayInfoTab()
    {
        if (!ImGui.BeginTabItem("Info"))
            return;
        
        ImGui.TextWrapped("Nothing is saved at the moment. All data is lost whenever you quit the game, so write down whatever you want to keep.");

        ImGui.EndTabItem();
    }
}
