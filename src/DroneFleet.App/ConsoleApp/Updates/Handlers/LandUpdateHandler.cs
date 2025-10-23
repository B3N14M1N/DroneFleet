using DroneFleet.App.ConsoleApp;
using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Updates.Handlers;

/// <summary>
/// Handles landing operations for supported drones.
/// </summary>
internal sealed class LandUpdateHandler : IDroneUpdateHandler
{
    public string Keyword => "land";

    public bool Supports(Drone drone) => drone is not null;

    public DroneUpdateResponse Execute(CommandContext context, Drone drone, IReadOnlyList<string> arguments)
    {
        if (arguments.Count > 0)
        {
            return new DroneUpdateResponse(Result.Failure("Land does not accept parameters.", ResultCodes.Validation), null);
        }

        var result = context.FleetService.Land(drone.Id);
        if (!result.IsSuccess)
        {
            return new DroneUpdateResponse(result, null);
        }

        return new DroneUpdateResponse(result, $"Drone {drone.Id} landed successfully.");
    }
}
