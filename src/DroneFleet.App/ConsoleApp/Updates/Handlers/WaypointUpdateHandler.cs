using System.Globalization;
using DroneFleet.App.ConsoleApp;
using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Updates.Handlers;

/// <summary>
/// Handles waypoint updates for navigable drones.
/// </summary>
internal sealed class WaypointUpdateHandler : IDroneUpdateHandler
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public string Keyword => "waypoint";

    public bool Supports(Drone drone) => drone is DeliveryDrone or SurveyDrone;

    public DroneUpdateResponse Execute(CommandContext context, Drone drone, IReadOnlyList<string> arguments)
    {
        if (arguments.Count < 2)
        {
            return new DroneUpdateResponse(Result.Failure("Waypoint update requires latitude and longitude.", ResultCodes.Validation), null);
        }

        if (!double.TryParse(arguments[0], NumberStyles.Float, Culture, out var latitude) ||
            !double.TryParse(arguments[1], NumberStyles.Float, Culture, out var longitude))
        {
            return new DroneUpdateResponse(Result.Failure("Invalid waypoint coordinates.", ResultCodes.Validation), null);
        }

        var result = context.FleetService.SetWaypoint(drone.Id, latitude, longitude);
        if (!result.IsSuccess)
        {
            return new DroneUpdateResponse(result, null);
        }

        return new DroneUpdateResponse(result, $"Drone {drone.Id} waypoint set to ({latitude}, {longitude}).");
    }
}
