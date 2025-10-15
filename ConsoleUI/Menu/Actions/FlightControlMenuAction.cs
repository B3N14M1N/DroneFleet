using DroneFleet.Services.Interfaces;

namespace DroneFleet.ConsoleUI.Menu.Actions;

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

        Console.WriteLine($"Current status: {(drone.IsAirborne ? "Airborne" : "Landed")}");

        var action = InputHelpers.PromptForOption("Action (1=TakeOff, 2=Land) (or press Enter to cancel): ", 2);
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
