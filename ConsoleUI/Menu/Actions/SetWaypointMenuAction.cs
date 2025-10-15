using DroneFleet.Models.Interfaces;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.ConsoleUI.Menu.Actions;

internal sealed class SetWaypointMenuAction(IDroneManager droneManager) : IMenuAction
{
    private readonly IDroneManager _droneManager = droneManager;

    public string Label => "Set waypoint";

    public string Description => "Assign a waypoint to a navigable drone";

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

        if (drone is not INavigable navigableDrone)
        {
            InputHelpers.PrintError($"{drone.Name} does not support navigation.");
            return MenuActionOutcome.Continue;
        }

        var coordinates = InputHelpers.PromptForCoordinates();
        if (coordinates == null)
        {
            return MenuActionOutcome.Continue;
        }

        try
        {
            var (lat, lon) = coordinates.Value;
            navigableDrone.SetWaypoint(lat, lon);
            InputHelpers.PrintSuccess($"Waypoint set to ({lat}, {lon}) for {drone.Name}. Battery: {drone.BatteryPercent}%");
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error setting waypoint: {ex.Message}");
        }

        return MenuActionOutcome.Continue;
    }
}
