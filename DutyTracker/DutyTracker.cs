using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using DutyTracker.Duty_Events;
using DutyTracker.Windows;

namespace DutyTracker;

public sealed class DutyTracker : IDalamudPlugin
{
    public        string Name => "DutyTracker";
    private const string CommandName = "/dt";

    private         DalamudPluginInterface PluginInterface { get; init; }
    private         CommandManager         CommandManager  { get; init; }
    public          Configuration          Configuration   { get; init; }
    public readonly WindowSystem           WindowSystem = new("DutyTracker");
    public readonly DutyManager            DutyManager;
    public readonly DutyEventManager       DutyEventManager;
    public readonly CombatEventCapture     CombatEventCapture;
    

    public DutyTracker(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager         commandManager)
    {
        this.PluginInterface = pluginInterface;
        this.CommandManager  = commandManager;

        this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.Configuration.Initialize(this.PluginInterface);

        DutyManager        = PluginInterface.Create<DutyManager>()!;
        DutyEventManager   = PluginInterface.Create<DutyEventManager>(DutyManager)!;
        CombatEventCapture = PluginInterface.Create<CombatEventCapture>(DutyManager)!;
        
        
        WindowSystem.AddWindow(new MainWindow(this, DutyManager));

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
        CombatEventCapture.Dispose();
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
