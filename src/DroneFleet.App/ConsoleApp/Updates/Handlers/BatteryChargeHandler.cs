using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;
using System.Globalization;

namespace DroneFleet.App.ConsoleApp.Updates.Handlers;

/// <summary>
/// Handles battery charge operations for any drone type.
/// </summary>
internal sealed class BatteryChargeHandler : IDroneUpdateHandler
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public string Keyword => "charge";

    public bool Supports(Drone drone) => drone is not null;

    public DroneUpdateResponse Execute(CommandContext context, Drone drone, IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return new DroneUpdateResponse(Result.Failure("Charge action requires a percentage value.", ResultCodes.Validation), null);
        }

        if (!double.TryParse(arguments[0], NumberStyles.Float, Culture, out var level))
        {
            return new DroneUpdateResponse(Result.Failure("Invalid charge percentage.", ResultCodes.Validation), null);
        }

        var result = context.FleetService.ChargeDrone(drone.Id, level);
        if (!result.IsSuccess)
        {
            return new DroneUpdateResponse(result, null);
        }

        var updated = context.FleetService.GetDrone(drone.Id);
        var message = updated.IsSuccess && updated.Value is not null
            ? $"Drone {drone.Id} charged. Battery level: {updated.Value.BatteryPercent}%"
            : $"Drone {drone.Id} charged.";

        return new DroneUpdateResponse(result, message);
    }
}
