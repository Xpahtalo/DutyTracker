using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using DutyTracker.Duty_Events;
using DutyTracker.Services;
using DutyTracker.Windows;

namespace DutyTracker;

using DutyTracker_Configuration = Configuration;

public sealed class DutyTracker : IDalamudPlugin
{
    public        string Name => "DutyTracker";
    private const string CommandName = "/dt";

    private         DalamudPluginInterface PluginInterface { get; init; }
    private         CommandManager         CommandManager  { get; init; }

    public          Configuration          Configuration   { get; init; }
    public readonly DutyManager            DutyManager;
    

    public DutyTracker(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager         commandManager)
    {
        PluginInterface = pluginInterface;
        CommandManager  = commandManager;

        PluginInterface.Create<Service>();
        Service.DutyEventService     = new DutyEventService();
        Service.PlayerCharacterState = new PlayerCharacterState();
        Service.WindowService        = new WindowService();
        
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        
        DutyManager        = PluginInterface.Create<DutyManager>(Configuration)!;

        Service.WindowService.AddWindow("MainWindow", new MainWindow(DutyManager, Configuration));
        Service.WindowService.AddWindow("DutyExplorer", new DutyExplorerWindow(DutyManager));
        Service.WindowService.AddWindow("Debug", new DebugWindow());

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
                                                    {
                                                        HelpMessage = "Open the Duty Tracker menu",
                                                    });

        PluginInterface.UiBuilder.Draw    += DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi += OpenSettings;
    }

    public void Dispose()
    {
        CommandManager.RemoveHandler(CommandName);
        Service.WindowService.Dispose();
        Service.PlayerCharacterState.Dispose();
        Service.DutyEventService.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        if (args == "debug")
            Service.WindowService.OpenWindow("Debug");
        else
            Service.WindowService.ToggleWindow("MainWindow");
    }

    private void OpenSettings()
    {
        Service.WindowService.OpenWindow("MainWindow");
    }
    
    private void DrawUi()
    {
        Service.WindowService.Draw();
    }
}
