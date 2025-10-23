using DroneFleet.Domain.Common;

namespace DroneFleet.Domain.Models;

/// <summary>
/// Represents a drone that can be tracked within the fleet.
/// Provides common behaviours shared across concrete drone types.
/// </summary>
public abstract class Drone(DroneKind kind)
{

    /// <summary>
    /// Gets or inits the unique identifier of the drone.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets or inits the human-readable name of the drone.
    /// </summary>
    public string Name { get; init; } = "Unnamed Drone";

    /// <summary>
    /// Gets the category of the drone.
    /// </summary>
    public DroneKind Kind { get; } = kind;

    /// <summary>
    /// Gets the current battery level expressed as a percentage (0-100).
    /// </summary>
    public double BatteryPercent { get; private set; } = 100.0;

    /// <summary>
    /// Gets a value indicating whether the drone is airborne.
    /// </summary>
    public bool IsAirborne { get; protected set; }

    /// <summary>
    /// Drains battery by the specified amount when enough charge is available.
    /// </summary>
    /// <param name="amount">The amount to drain from battery.</param>
    protected Result DrainBattery(double amount)
    {
        if (double.IsNaN(amount) || amount <= 0)
        {
            return Result.Failure("Drain amount must be a positive number.", ResultCodes.Validation);
        }

        if (BatteryPercent < amount)
        {
            return Result.Failure("Insufficient battery for the requested operation.", ResultCodes.Validation);
        }

        var result = BatteryPercent - amount;
        BatteryPercent = Math.Round(result, 2, MidpointRounding.AwayFromZero);
        return Result.Success();
    }

    /// <summary>
    /// Marks the drone as landed.
    /// </summary>
    public virtual void Land()
    {
        IsAirborne = false;
    }

    /// <summary>
    /// Performs a minimal self-test to indicate readiness for operations.
    /// </summary>
    public virtual bool RunSelfTest()
    {
        return BatteryPercent >= 20.0f;
    }

    /// <summary>
    /// Marks the drone as airborne if the basic requirements are met.
    /// </summary>
    public virtual Result TakeOff()
    {
        if (BatteryPercent < 20.0f)
        {
            return Result.Failure("Insufficient battery for take-off. Minimum 20% required.", ResultCodes.Validation);
        }

        if (IsAirborne)
        {
            return Result.Failure("Drone is already airborne.", ResultCodes.Validation);
        }

        var drainResult = DrainBattery(5.0f);
        if (!drainResult.IsSuccess)
        {
            return drainResult;
        }

        IsAirborne = true;
        return Result.Success();
    }

    /// <summary>
    /// Attempts to set the battery percentage to a new absolute value.
    /// </summary>
    /// <param name="value">The new battery percentage.</param>
    /// <returns>A result indicating whether the update succeeded.</returns>
    public Result SetBatteryPercent(double value)
    {
        if (double.IsNaN(value) || value < 0 || value > 100)
        {
            return Result.Failure("Battery percent must be between 0 and 100.", ResultCodes.Validation);
        }

        BatteryPercent = Math.Round(value, 2, MidpointRounding.AwayFromZero);
        return Result.Success();
    }

    /// <summary>
    /// Applies telemetry values retrieved from an external source.
    /// </summary>
    public Result ApplyTelemetry(double batteryPercent, bool isAirborne)
    {
        var batteryResult = SetBatteryPercent(batteryPercent);
        if (!batteryResult.IsSuccess)
        {
            return batteryResult;
        }

        IsAirborne = isAirborne;
        return Result.Success();
    }

    /// <summary>
    /// Increases the battery by the specified amount while enforcing the 0-100 range.
    /// </summary>
    /// <param name="charge">The charge to apply.</param>
    /// <returns>A result indicating whether the operation succeeded.</returns>
    public Result ChargeBattery(double charge)
    {
        if (double.IsNaN(charge) || charge < 0)
        {
            return Result.Failure("Charge amount must be a non-negative number.", ResultCodes.Validation);
        }

        var newLevel = Math.Min(100.0, BatteryPercent + charge);
        BatteryPercent = Math.Round(newLevel, 2, MidpointRounding.AwayFromZero);
        return Result.Success();
    }

    public override string ToString()
    {
        return $"ID: {Id}, Name: {Name}, Kind: {Kind}, Battery: {BatteryPercent}%, Airborne: {IsAirborne}";
    }
}
