namespace DroneFleet.ConsoleUI.Menu.Actions;

internal sealed class ExitMenuAction : IMenuAction
{
    public string Label => "Exit";

    public string Description => "Close the application";

    public MenuActionOutcome Execute()
    {
        Console.WriteLine("\nExiting Drone Fleet Management System. Goodbye!");
        return MenuActionOutcome.Exit;
    }
}
