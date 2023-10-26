using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace DutyTracker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? PluginInterface;

    public int Version { get; set; } = 0;

    public bool IncludeDutyTrackerLabel { get; set; } = false;
    public bool SuppressEmptyValues     { get; set; } = true;

    public void Initialize(DalamudPluginInterface pluginInterface) { PluginInterface = pluginInterface; }

    public void Save() { PluginInterface!.SavePluginConfig(this); }
}
