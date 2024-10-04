using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace DutyTracker.Windows.Config;

public partial class ConfigWindow : Window, IDisposable
{
    private readonly DutyTracker DutyTracker;

    public ConfigWindow(DutyTracker dutyTracker) : base("DutyTracker settings")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(320, 460),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        DutyTracker = dutyTracker;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var tabBar = ImRaii.TabBar("##ConfigTabBar");
        if (!tabBar.Success)
            return;

        General();

        About();
    }
}
