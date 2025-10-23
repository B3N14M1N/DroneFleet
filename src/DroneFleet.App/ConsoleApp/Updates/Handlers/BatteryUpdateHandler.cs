using System.Globalization;
using DroneFleet.App.ConsoleApp;
using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Updates.Handlers;

/// <summary>
/// Handles battery update operations for any drone type.
/// </summary>
internal sealed class BatteryUpdateHandler : IDroneUpdateHandler
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public string Keyword => "battery";

    public bool Supports(Drone drone) => drone is not null;

    public DroneUpdateResponse Execute(CommandContext context, Drone drone, IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return new DroneUpdateResponse(Result.Failure("Battery update requires a percentage value.", ResultCodes.Validation), null);
        }

        if (!double.TryParse(arguments[0], NumberStyles.Float, Culture, out var level))
        {
            return new DroneUpdateResponse(Result.Failure("Invalid battery percentage.", ResultCodes.Validation), null);
        }

        var result = context.FleetService.UpdateBattery(drone.Id, level);
        if (!result.IsSuccess)
        {
            return new DroneUpdateResponse(result, null);
        }

        var updated = context.FleetService.GetDrone(drone.Id);
        var message = updated.IsSuccess && updated.Value is not null
            ? $"Drone {drone.Id} battery set to {updated.Value.BatteryPercent}%"
            : $"Drone {drone.Id} battery updated.";

        return new DroneUpdateResponse(result, message);
    }
}
