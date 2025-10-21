namespace DroneFleet.Domain.Models;

public class RacingDrone : Drone
{
    private const double TAKEOFF_BATTERY_DRAIN = 3.0f;
    /// <inheritdoc/>
    public override void TakeOff()
    {
        if (BatteryPercent < 20.0f)
        {
            throw new InvalidOperationException("Insufficient battery for takeoff. Minimum 20% required.");
        }

        if (IsAirborne)
        {
            throw new InvalidOperationException("Drone is already airborne.");
        }

        IsAirborne = true;
        DrainBattery(TAKEOFF_BATTERY_DRAIN); 
    }
}
