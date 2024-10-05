using ImGuiNET;

namespace DutyTracker.Windows;

public static class Helper
{
    internal static void InfoText(string label, string value) => InfoText(label, value, ImGui.GetContentRegionAvail().X);

    internal static void InfoText(string label, string value, float width)
    {
        ImGui.TextUnformatted(label);
        ImGui.SameLine();
        ImGui.SetCursorPosX(width - ImGui.CalcTextSize(value).X);
        ImGui.TextUnformatted(value);
    }

    internal static void TableHeader(params string[] columnTitles)
    {
        foreach (var title in columnTitles)
            ImGui.TableSetupColumn(title);

        ImGui.TableHeadersRow();
        ImGui.TableNextRow();
    }

    internal static void TableRow(params string[] values)
    {
        ImGui.TableNextRow();
        for (var i = 0; i < values.Length; i++)
        {
            ImGui.TableSetColumnIndex(i);
            ImGui.TextUnformatted(values[i]);
        }
    }
}