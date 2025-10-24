using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Updates.Handlers;

/// <summary>
/// Handles unloading cargo for delivery drones.
/// </summary>
internal sealed class CargoUnloadUpdateHandler : IDroneUpdateHandler
{
    public string Keyword => "unload";

    public bool Supports(Drone drone) => drone is DeliveryDrone;

    public DroneUpdateResponse Execute(CommandContext context, Drone drone, IReadOnlyList<string> arguments)
    {
        if (arguments.Count > 0)
        {
            return new DroneUpdateResponse(Result.Failure("Unload does not accept parameters.", ResultCodes.Validation), null);
        }

        var result = context.FleetService.UnloadCargo(drone.Id);
        if (!result.IsSuccess)
        {
            return new DroneUpdateResponse(result, null);
        }

        return new DroneUpdateResponse(result, $"Drone {drone.Id} cargo unloaded.");
    }
}
