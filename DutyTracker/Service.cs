using System.Diagnostics.CodeAnalysis;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.DutyState;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using DutyTracker.Services;

namespace DutyTracker;
#pragma warning disable CS8618
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public class Service
{
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static ChatGui                ChatGui         { get; private set; }
    [PluginService] public static ClientState            ClientState     { get; private set; }
    [PluginService] public static PartyList              PartyList       { get; private set; }
    [PluginService] public static CommandManager         CommandManager  { get; private set; }
    [PluginService] public static Condition              Condition       { get; private set; }
    [PluginService] public static DataManager            DataManager     { get; private set; }
    [PluginService] public static Framework              Framework       { get; private set; }
    [PluginService] public static ObjectTable            ObjectTable     { get; private set; }
    [PluginService] public static GameGui                GameGui         { get; private set; }
    [PluginService] public static DutyState              DutyState       { get; private set; }

    internal static DutyEventService     DutyEventService;
    internal static PlayerCharacterState PlayerCharacterState;
    internal static WindowService        WindowService;
}
#pragma warning restore CS8618