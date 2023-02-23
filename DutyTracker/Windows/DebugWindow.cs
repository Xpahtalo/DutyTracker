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
            DisplayFFXIVDebug();
            DisplayPlayerCharacterStateDebug();
        }

        ImGui.EndTabBar();
    }

    private void DisplayFFXIVDebug()
    {
        if (!ImGui.BeginTabItem("Group"))
            return;
        
        Service.PlayerCharacterState.ImGuiParty();
        
        ImGui.EndTabItem();
    }

    private void DisplayPlayerCharacterStateDebug()
    {
        if (!ImGui.BeginTabItem("PlayerCharacterState"))
            return;
     
        Service.PlayerCharacterState.DebugCache();

        ImGui.EndTabItem();
    }
}
