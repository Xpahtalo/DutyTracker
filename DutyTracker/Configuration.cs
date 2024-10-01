using System;
using Dalamud.Configuration;

namespace DutyTracker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IncludeDutyTrackerLabel = false;
    public bool SuppressEmptyValues = true;

    public void Save()
    {
        DutyTracker.PluginInterface.SavePluginConfig(this);
    }
}