using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace DutyTracker.Windows.Config;

public partial class ConfigWindow
{
    private const float SeparatorPadding = 1.0f;
    private float GetSeparatorPaddingHeight() => SeparatorPadding * ImGuiHelpers.GlobalScale;

    private void About()
    {
        using var tabItem = ImRaii.TabItem("About");
        if (!tabItem.Success)
            return;

        var bottomContentHeight = ImGui.GetTextLineHeightWithSpacing() + (ImGui.GetStyle().FramePadding.Y * 2) + ImGui.GetStyle().WindowPadding.Y + GetSeparatorPaddingHeight();
        using (var contentChild = ImRaii.Child("AboutContent", new Vector2(0, -bottomContentHeight)))
        {
            if (contentChild.Success)
            {
                ImGuiHelpers.ScaledDummy(5.0f);

                ImGui.TextUnformatted("Author:");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, DutyTracker.PluginInterface.Manifest.Author);

                ImGui.TextUnformatted("Discord:");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, "@infi");

                ImGui.TextUnformatted("Version:");
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedOrange, DutyTracker.PluginInterface.Manifest.AssemblyVersion.ToString());
            }
        }

        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(SeparatorPadding);

        using var bottomChild = ImRaii.Child("AboutBottomBar", new Vector2(0, 0), false, 0);
        if (!bottomChild.Success)
            return;

        using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.ParsedBlue))
        {
            if (ImGui.Button("Discord Thread"))
                Dalamud.Utility.Util.OpenLink("https://discord.com/channels/581875019861328007/1055009775206805565");
        }

        ImGui.SameLine();

        using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.DPSRed))
        {
            if (ImGui.Button("Issues"))
                Dalamud.Utility.Util.OpenLink("https://github.com/Infiziert90/DutyTracker/issues");
        }

        ImGui.SameLine();

        using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.12549f, 0.74902f, 0.33333f, 0.6f)))
        {
            if (ImGui.Button("Ko-Fi Tip"))
                Dalamud.Utility.Util.OpenLink("https://ko-fi.com/infiii");
        }
    }
}