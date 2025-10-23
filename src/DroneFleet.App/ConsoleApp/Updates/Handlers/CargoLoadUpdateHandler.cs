using System.Globalization;
using DroneFleet.App.ConsoleApp;
using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Updates.Handlers;

/// <summary>
/// Handles cargo load updates for delivery drones.
/// </summary>
internal sealed class CargoLoadUpdateHandler : IDroneUpdateHandler
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public string Keyword => "load";

    public bool Supports(Drone drone) => drone is DeliveryDrone;

    public DroneUpdateResponse Execute(CommandContext context, Drone drone, IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return new DroneUpdateResponse(Result.Failure("Load update requires a weight in kilograms.", ResultCodes.Validation), null);
        }

        if (!double.TryParse(arguments[0], NumberStyles.Float, Culture, out var loadKg))
        {
            return new DroneUpdateResponse(Result.Failure("Invalid cargo weight.", ResultCodes.Validation), null);
        }

        var result = context.FleetService.UpdateCargoLoad(drone.Id, loadKg);
        if (!result.IsSuccess)
        {
            return new DroneUpdateResponse(result, null);
        }

        var updated = context.FleetService.GetDrone(drone.Id);
        if (updated.IsSuccess && updated.Value is DeliveryDrone delivery)
        {
            var message = $"Drone {delivery.Id} load set to {delivery.CurrentLoadKg}/{delivery.CapacityKg} kg.";
            return new DroneUpdateResponse(result, message);
        }

        return new DroneUpdateResponse(result, $"Drone {drone.Id} load updated.");
    }
}
