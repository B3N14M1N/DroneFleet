using DroneFleet.Domain.Common;

namespace DroneFleet.Domain.Models;

/// <summary>
/// Represents a high-speed racing drone.
/// </summary>
public sealed class RacingDrone : Drone
{
    private const double TAKEOFF_BATTERY_DRAIN = 3.0f;

    public RacingDrone() : base(DroneKind.Racing)
    {
    }

    /// <inheritdoc/>
    public override Result TakeOff()
    {
        if (BatteryPercent < 20.0f)
        {
            return Result.Failure("Insufficient battery for take-off. Minimum 20% required.", ResultCodes.Validation);
        }

        if (IsAirborne)
        {
            return Result.Failure("Drone is already airborne.", ResultCodes.Validation);
        }

        var drainResult = DrainBattery(TAKEOFF_BATTERY_DRAIN);
        if (!drainResult.IsSuccess)
        {
            return drainResult;
        }

        IsAirborne = true;
        return Result.Success();
    }
}
