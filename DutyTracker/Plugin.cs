using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using DutyTracker.Windows;

namespace DutyTracker;

public sealed class Plugin : IDalamudPlugin
{
    public        string Name => "DutyTracker";
    private const string CommandName = "/dt";

    private         DalamudPluginInterface PluginInterface { get; init; }
    private         CommandManager         CommandManager  { get; init; }
    public          Configuration          Configuration   { get; init; }
    public readonly WindowSystem           WindowSystem = new("DutyTracker");
    public readonly DutyEventManager       DutyEventManager;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager         commandManager)
    {
        this.PluginInterface = pluginInterface;
        this.CommandManager  = commandManager;

        this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.Configuration.Initialize(this.PluginInterface);

        DutyEventManager = PluginInterface.Create<DutyEventManager>();
        
        WindowSystem.AddWindow(new MainWindow(this, DutyEventManager));

        this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
                                                    {
                                                        HelpMessage = "Open the Duty Tracker menu",
                                                    });

        this.PluginInterface.UiBuilder.Draw += DrawUi;
    }

    public void Dispose()
    {
        this.WindowSystem.RemoveAllWindows();
        this.CommandManager.RemoveHandler(CommandName);
        DutyEventManager.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just display our main ui
        WindowSystem.GetWindow("Duty Tracker")!.IsOpen = true;
    }

    private void DrawUi()
    {
        this.WindowSystem.Draw();
    }
}
