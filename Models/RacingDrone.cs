namespace DroneFleet.Models;

internal class RacingDrone : Drone
{
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
        DrainBattery(3.0f); 
    }
}
