using ImGuiNET;

namespace DutyTracker;

internal static class XGui
{
    internal static void InfoText(string label, string value) => InfoText(label, value, ImGui.GetContentRegionAvail().X);

    internal static void InfoText(string label, string value, float width)
    {
        var valueX = width - ImGui.CalcTextSize(value).X;
        ImGui.Text(label);
        ImGui.SameLine();
        ImGui.SetCursorPosX(valueX);
        ImGui.Text(value);
    }

    internal static void TableHeader(params string[] columnTitles)
    {
        foreach (var title in columnTitles) ImGui.TableSetupColumn(title);
        ImGui.TableHeadersRow();
        ImGui.TableNextRow();
    }

    internal static void TableRow(params string[] values)
    {
        ImGui.TableNextRow();
        for (var i = 0; i < values.Length; i++) {
            ImGui.TableSetColumnIndex(i);
            ImGui.TextUnformatted(values[i]);
        }
    }
}
