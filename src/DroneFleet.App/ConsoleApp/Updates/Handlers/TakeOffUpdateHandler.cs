using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Updates.Handlers;

/// <summary>
/// Handles take-off operations for supported drones.
/// </summary>
internal sealed class TakeOffUpdateHandler : IDroneUpdateHandler
{
    public string Keyword => "takeoff";

    public bool Supports(Drone drone) => drone is not null;

    public DroneUpdateResponse Execute(CommandContext context, Drone drone, IReadOnlyList<string> arguments)
    {
        if (arguments.Count > 0)
        {
            return new DroneUpdateResponse(Result.Failure("Takeoff does not accept parameters.", ResultCodes.Validation), null);
        }

        var result = context.FleetService.TakeOff(drone.Id);
        if (!result.IsSuccess)
        {
            return new DroneUpdateResponse(result, null);
        }

        return new DroneUpdateResponse(result, $"Drone {drone.Id} is now airborne.");
    }
}
