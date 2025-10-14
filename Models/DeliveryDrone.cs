using DroneFleet.Models.Interfaces;

namespace DroneFleet.Models;

internal class DeliveryDrone : Drone, INavigable, ICargoCarrier
{
    public (double lat, double lon)? CurrentWaypoint { get; private set; }

    public double CapacityKg { get; init; }

    public double CurrentLoadKg { get; private set; }

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

        float takeoffDrain = 5.0f + (float)(CurrentLoadKg / 10.0);
        DrainBattery(takeoffDrain);
    }

    /// <inheritdoc/>
    public bool Load(double kg)
    {
        if (kg <= 0 || CurrentLoadKg + kg > CapacityKg) return false;

        if (IsAirborne) throw new InvalidOperationException("Cannot load while airborne.");

        CurrentLoadKg += kg;

        return true;
    }

    /// <inheritdoc/>
    public void SetWaypoint(double lat, double lon)
    {
        CurrentWaypoint = (lat, lon);

        float navigationDrain = 2.0f + (float)(CurrentLoadKg / 20.0);
        DrainBattery(navigationDrain);
    }

    /// <inheritdoc/>
    public void UnloadAll()
    {
        if (!IsAirborne)
        {
            CurrentLoadKg = 0;
        }
        else
        {
            throw new InvalidOperationException("Cannot unload while airborne.");
        }
    }

    public override string ToString()
    {
        return base.ToString() + "," +
            $" Waypoint: {(this.CurrentWaypoint != null ? this.CurrentWaypoint : "None")}," +
            $" Max Capacity: {this.CapacityKg}kg," +
            $" Loaded: {this.CurrentLoadKg}kg";
    }
}
