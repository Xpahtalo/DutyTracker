using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;

namespace DutyTracker.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private Plugin           Plugin;
    private DutyEventManager DutyEventManager;

    public MainWindow(Plugin plugin, DutyEventManager dutyEventManager) : base(
        "Duty Tracker", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        this.Plugin      = plugin;
        DutyEventManager = dutyEventManager;
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

        ImGui.Text($"Start Time: {DutyEventManager.StartTime}");
        ImGui.Text($"End Time: {DutyEventManager.EndTime}");
        ImGui.Text($"Elapsed time: {DutyEventManager.ElapsedTime}");
        ImGui.Text($"Duty Status: {DutyEventManager.DutyStarted}");
        

        ImGui.EndTabItem();
    }

    private void DisplayOptions()
    {
        if (!ImGui.BeginTabItem("Options"))
            return;
        ImGui.EndTabItem();
    }
}
