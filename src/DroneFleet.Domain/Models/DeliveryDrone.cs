using DroneFleet.Domain.Models.Interfaces;

namespace DroneFleet.Domain.Models;

public class DeliveryDrone : Drone, INavigable, ICargoCarrier
{
    private const double BASE_TAKEOFF_DRAIN = 5.0;
    private const double BASE_NAVIGATION_DRAIN = 2.0;
    private const double LOAD_DRAIN_FACTOR = 0.1;

    public (double lat, double lon)? CurrentWaypoint { get; private set; }

    public double CapacityKg { get; init; }

    public double CurrentLoadKg { get; private set; }

    /// <inheritdoc/>
    public override void TakeOff()
    {
        double takeoffDrain = Math.Round(BASE_TAKEOFF_DRAIN + CurrentLoadKg * LOAD_DRAIN_FACTOR, 2);

        if (BatteryPercent < 20.0 || BatteryPercent < takeoffDrain)
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
    public override bool RunSelfTest()
    {
        double takeoffDrain = Math.Round(BASE_TAKEOFF_DRAIN + CurrentLoadKg * LOAD_DRAIN_FACTOR, 2);
        return BatteryPercent >= Math.Max(20.0, takeoffDrain);
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
        double navigationDrain = Math.Round(BASE_NAVIGATION_DRAIN + CurrentLoadKg * LOAD_DRAIN_FACTOR, 2);

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
            $" Waypoint: {(CurrentWaypoint != null ? CurrentWaypoint : "None")}," +
            $" Max Capacity: {CapacityKg}kg," +
            $" Loaded: {CurrentLoadKg}kg";
    }
}
