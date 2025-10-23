using DroneFleet.Domain.Common;

namespace DroneFleet.Domain.Models;

/// <summary>
/// Represents a drone specialised in transporting cargo.
/// </summary>
public sealed class DeliveryDrone(double capacityKg = DeliveryDrone.DEFAULT_CAPACITY_KG) : Drone(DroneKind.Delivery)
{
    private const double BASE_TAKEOFF_DRAIN = 5.0;
    private const double BASE_NAVIGATION_DRAIN = 2.0;
    private const double LOAD_DRAIN_FACTOR = 0.1;
    private const double DEFAULT_CAPACITY_KG = 10.0;

    public (double lat, double lon)? CurrentWaypoint { get; private set; }

    public double CapacityKg { get; private set; } = Math.Max(DEFAULT_CAPACITY_KG, capacityKg);

    public double CurrentLoadKg { get; private set; }

    /// <summary>
    /// Applies a persisted snapshot to the delivery drone without performing side effects.
    /// </summary>
    /// <param name="loadKg">The current load in kilograms.</param>
    /// <param name="waypoint">The waypoint to restore.</param>
    internal void ApplySnapshot(double? loadKg, (double lat, double lon)? waypoint)
    {
        var load = loadKg.HasValue ? Math.Max(0, loadKg.Value) : 0;
        if (load > CapacityKg)
        {
            CapacityKg = Math.Max(load, CapacityKg);
        }

        CurrentLoadKg = Math.Round(Math.Min(load, CapacityKg), 2, MidpointRounding.AwayFromZero);
        CurrentWaypoint = waypoint;
    }

    /// <inheritdoc/>
    public override Result TakeOff()
    {
        double takeoffDrain = Math.Round(BASE_TAKEOFF_DRAIN + CurrentLoadKg * LOAD_DRAIN_FACTOR, 2);

        if (BatteryPercent < 20.0 || BatteryPercent < takeoffDrain)
        {
            return Result.Failure($"Insufficient battery for take-off. Minimum {Math.Max(20, takeoffDrain)}% required.", ResultCodes.Validation);
        }

        if (IsAirborne)
        {
            return Result.Failure("Drone is already airborne.", ResultCodes.Validation);
        }

        var drainResult = DrainBattery(takeoffDrain);
        if (!drainResult.IsSuccess)
        {
            return drainResult;
        }

        IsAirborne = true;
        return Result.Success();
    }

    /// <inheritdoc/>
    public override bool RunSelfTest()
    {
        double takeoffDrain = Math.Round(BASE_TAKEOFF_DRAIN + CurrentLoadKg * LOAD_DRAIN_FACTOR, 2);
        return BatteryPercent >= Math.Max(20.0, takeoffDrain);
    }

    /// <summary>
    /// Updates the current load of the drone.
    /// </summary>
    public Result UpdateLoad(double kg)
    {
        if (kg < 0 || kg > CapacityKg)
        {
            return Result.Failure($"Load must be between 0 and {CapacityKg} kg.", ResultCodes.Validation);
        }

        if (IsAirborne)
        {
            return Result.Failure("Cannot modify load while airborne.", ResultCodes.Validation);
        }

        CurrentLoadKg = Math.Round(kg, 2, MidpointRounding.AwayFromZero);

        return Result.Success();
    }

    /// <summary>
    /// Assigns a waypoint for the delivery drone, draining battery accordingly.
    /// </summary>
    public Result SetWaypoint(double lat, double lon)
    {
        double navigationDrain = Math.Round(BASE_NAVIGATION_DRAIN + CurrentLoadKg * LOAD_DRAIN_FACTOR, 2);

        if (BatteryPercent < navigationDrain)
        {
            return Result.Failure($"Insufficient battery for movement. Minimum {navigationDrain}% required.", ResultCodes.Validation);
        }

        var drainResult = DrainBattery(navigationDrain);
        if (!drainResult.IsSuccess)
        {
            return drainResult;
        }

        CurrentWaypoint = (lat, lon);
        return Result.Success();
    }

    /// <summary>
    /// Clears the loaded cargo when the drone is grounded.
    /// </summary>
    public Result UnloadAll()
    {
        if (!IsAirborne)
        {
            CurrentLoadKg = 0;
            return Result.Success();
        }

        return Result.Failure("Cannot unload while airborne.", ResultCodes.Validation);
    }

    public override string ToString()
    {
        return base.ToString() + "," +
            $" Waypoint: {(CurrentWaypoint != null ? CurrentWaypoint : "None")}," +
            $" Max Capacity: {CapacityKg}kg," +
            $" Loaded: {CurrentLoadKg}kg";
    }
}
