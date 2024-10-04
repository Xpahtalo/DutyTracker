using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using DutyTracker.Extensions;
using ImGuiNET;

namespace DutyTracker.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private readonly DutyTracker DutyTracker;

    public MainWindow(DutyTracker dutyTracker) : base("Duty Tracker")
    {
        Flags = ImGuiWindowFlags.AlwaysAutoResize;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(248, 250),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        DutyTracker = dutyTracker;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        using var tabBar = ImRaii.TabBar("MainWindowTabBar");
        if (!tabBar.Success)
            return;

        DisplayStatusTab();
        DisplayInfoTab();
    }

    private void DisplayStatusTab()
    {
        using var tabItem = ImRaii.TabItem("Status");
        if (!tabItem.Success)
            return;

        var newestDuty = DutyTracker.DutyManager.GetMostRecentDuty();
        var newestRun = DutyTracker.DutyManager.GetMostRecentRun();
        if (newestDuty is not null)
        {
            Helper.InfoText("Start Time:", $@"{newestDuty.StartTime:hh\:mm\:ss tt}");
            Helper.InfoText("Start of Current Run:", $@"{newestDuty.StartTime:hh\:mm\:ss tt}");
            if (DutyTracker.DutyManager.DutyActive)
            {
                Helper.InfoText("Elapsed Time:", $"{newestDuty.Duration.MinutesAndSeconds()}");
                if (newestRun is not null)
                    Helper.InfoText("Current Run Time:", $"{newestRun.Duration.MinutesAndSeconds()}");
            }
            else
            {
                Helper.InfoText("Total Duty Time:", $"{newestDuty.Duration.MinutesAndSeconds()}");
                if (newestRun is not null)
                    Helper.InfoText("Final Run Time:", $"{newestRun.Duration.MinutesAndSeconds()}");
            }

            Helper.InfoText("In Duty:", $"{DutyTracker.DutyManager.DutyActive}");
            Helper.InfoText("Deaths:", $"{newestDuty.TotalDeaths}");
            Helper.InfoText("Wipes:", $"{newestDuty.TotalWipes}");
        }
        else
        {
            // This only happens if no duties have been started since the plugin loaded.
            ImGui.TextUnformatted("No duties");
        }


        if (ImGui.Button("Open Explorer"))
            DutyTracker.WindowService.ToggleWindow("DutyExplorer");
#if DEBUG
        if (ImGui.Button("Open Debug"))
            DutyTracker.WindowService.ToggleWindow("Debug");
#endif
    }

    private void DisplayInfoTab()
    {
        using var tabItem = ImRaii.TabItem("Info");
        if (!tabItem.Success)
            return;

        ImGui.TextWrapped("Nothing is saved at the moment. All data is lost whenever you quit the game, so write down whatever you want to keep.");
    }
}