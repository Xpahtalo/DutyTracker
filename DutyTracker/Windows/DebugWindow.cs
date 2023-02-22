using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace DutyTracker.Windows;

public class DebugWindow : Window, IDisposable
{
    public DebugWindow()
        : base("Debug")
    {
        
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("DebugTabBar"))
        {
            DisplayGroupDebug();
        }

        ImGui.EndTabBar();
    }

    private void DisplayGroupDebug()
    {
        if (!ImGui.BeginTabItem("Group"))
            return;
        
        Service.PlayerCharacterState.ImGuiParty();
        
        ImGui.EndTabItem();
    }
}
