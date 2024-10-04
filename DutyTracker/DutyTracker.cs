using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DutyTracker.DutyEvents;
using DutyTracker.Services;
using DutyTracker.Services.DutyEvent;
using DutyTracker.Services.PlayerCharacter;
using DutyTracker.Windows;
using DutyTracker.Windows.Config;

namespace DutyTracker;

public sealed class DutyTracker : IDalamudPlugin
{
    [PluginService] public static IDataManager Data { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static ICommandManager Commands { get; private set; } = null!;
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IDutyState DutyState { get; private set; } = null!;

    private const string CommandName = "/dt";

    public readonly Configuration Configuration;
    public readonly DutyManager DutyManager;

    internal static DutyEventService DutyEventService = null!;
    internal static PlayerCharacterState PlayerCharacterState = null!;
    internal static WindowService WindowService = null!;

    public DutyTracker()
    {
        DutyEventService = new DutyEventService();
        PlayerCharacterState = new PlayerCharacterState();
        WindowService = new WindowService();

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        DutyManager = new DutyManager(this);

        WindowService.AddWindow("MainWindow", new MainWindow(this));
        WindowService.AddWindow("ConfigWindow", new ConfigWindow(this));
        WindowService.AddWindow("DutyExplorer", new DutyExplorerWindow(this));
        WindowService.AddWindow("Debug", new DebugWindow());

        Commands.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Duty Tracker menu",
        });

        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenMainUi += OpenMain;
        PluginInterface.UiBuilder.OpenConfigUi += OpenSettings;

#if DEBUG
        // Service.DutyEventService.Debug();
#endif
    }

    public void Dispose()
    {
        Commands.RemoveHandler(CommandName);
        WindowService.Dispose();

        PlayerCharacterState.Dispose();
        DutyEventService.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        if (args == "debug")
            WindowService.OpenWindow("Debug");
        else
            WindowService.ToggleWindow("MainWindow");
    }

    private void OpenMain() => WindowService.OpenWindow("MainWindow");
    private void OpenSettings() => WindowService.OpenWindow("ConfigWindow");

    private void DrawUi()
    {
        WindowService.Draw();
    }
}