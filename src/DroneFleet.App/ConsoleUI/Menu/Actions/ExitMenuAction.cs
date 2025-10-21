namespace DroneFleet.App.ConsoleUI.Menu.Actions;

internal sealed class ExitMenuAction : IMenuAction
{
    public string Label => "Exit";

    /// <inheritdoc/>
    public MenuActionOutcome Execute()
    {
        Console.WriteLine("\nExiting Drone Fleet Management System. Goodbye!");
        return MenuActionOutcome.Exit;
    }
}
