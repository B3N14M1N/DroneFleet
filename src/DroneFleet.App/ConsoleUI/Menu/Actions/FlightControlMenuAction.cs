using DroneFleet.Infrastructure.Services.Interfaces;

namespace DroneFleet.App.ConsoleUI.Menu.Actions;

internal sealed class FlightControlMenuAction(IDroneManager droneManager) : IMenuAction
{
    private readonly IDroneManager _droneManager = droneManager;

    public string Label => "Take off / Land";

    /// <inheritdoc/>
    public MenuActionOutcome Execute()
    {
        ArgumentNullException.ThrowIfNull(_droneManager);

        Console.WriteLine();
        var drone = InputHelpers.PromptForDrone(_droneManager);
        if (drone == null)
        {
            return MenuActionOutcome.Continue;
        }

        Console.WriteLine();
        Console.WriteLine($"Current status: {(drone.IsAirborne ? "Airborne" : "Landed")}");
        Console.WriteLine("Actions:");

        var flightActions = new[] { "TakeOff", "Land" };
        for (int i = 0; i < flightActions.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {flightActions[i]}");
        }

        var action = InputHelpers.PromptForOption(
            "Enter action (number or name) (or press Enter to cancel): ",
            2,
            validStrings: flightActions);

        if (action == null)
        {
            return MenuActionOutcome.Continue;
        }

        try
        {
            switch (action.Value)
            {
                case 1:
                    drone.TakeOff();
                    InputHelpers.PrintSuccess($"{drone.Name} took off. Battery: {drone.BatteryPercent}%");
                    break;
                case 2:
                    drone.Land();
                    InputHelpers.PrintSuccess($"{drone.Name} landed. Battery: {drone.BatteryPercent}%");
                    break;
            }
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error: {ex.Message}");
        }

        return MenuActionOutcome.Continue;
    }
}
