using DroneFleet.Models.Interfaces;

namespace DroneFleet.Models;

internal class DeliveryDrone : Drone, INavigable, ICargoCarrier
{
    private const double BASE_TAKEOFF_DRAIN = 5.0f;
    private const double BASE_NAVIGATION_DRAIN = 2.0f;
    private const double LOAD_DRAIN_FACTOR = 0.1f;

    public (double lat, double lon)? CurrentWaypoint { get; private set; }

    public double CapacityKg { get; init; }

    public double CurrentLoadKg { get; private set; }

    /// <inheritdoc/>
    public override void TakeOff()
    {
        double takeoffDrain = BASE_TAKEOFF_DRAIN + (CurrentLoadKg * LOAD_DRAIN_FACTOR);

        if (BatteryPercent < 20.0f || BatteryPercent < takeoffDrain)
        {
            throw new InvalidOperationException($"Insufficient battery for takeoff." +
                $" Minimum {Math.Max(20, takeoffDrain)}% required.");
        }

        if (IsAirborne)
        {
            throw new InvalidOperationException("Drone is already airborne.");
        }

        DrainBattery(takeoffDrain);
        IsAirborne = true;
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
        double navigationDrain = BASE_NAVIGATION_DRAIN + (CurrentLoadKg * LOAD_DRAIN_FACTOR);

        if (BatteryPercent < navigationDrain)
        {
            throw new InvalidOperationException($"Insufficient battery for movement." +
                $" Minimum {navigationDrain}% required.");
        }

        CurrentWaypoint = (lat, lon);
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
