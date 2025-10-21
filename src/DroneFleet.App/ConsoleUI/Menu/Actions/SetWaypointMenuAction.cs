using DroneFleet.Domain.Models.Interfaces;
using DroneFleet.Infrastructure.Services.Interfaces;

namespace DroneFleet.App.ConsoleUI.Menu.Actions;

internal sealed class SetWaypointMenuAction(IDroneManager droneManager) : IMenuAction
{
    private readonly IDroneManager _droneManager = droneManager;

    public string Label => "Set waypoint";

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

        var lat = InputHelpers.PromptForDouble("Enter latitude (or press Enter to cancel):");
        if (lat == null)
        {
            return MenuActionOutcome.Continue;
        }

        var lon = InputHelpers.PromptForDouble("Enter longitude (or press Enter to cancel):");
        if (lon == null)
        {
            return MenuActionOutcome.Continue;
        }

        try
        {
            navigableDrone.SetWaypoint(lat.Value, lon.Value);
            InputHelpers.PrintSuccess($"Waypoint set to ({lat.Value}, {lon.Value}) for {drone.Name}. Battery: {drone.BatteryPercent}%");
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error setting waypoint: {ex.Message}");
        }

        return MenuActionOutcome.Continue;
    }
}
