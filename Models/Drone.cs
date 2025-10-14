using DroneFleet.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace DroneFleet.Models;

internal abstract class Drone : IFlightControl, ISelfTest
{
    public int Id { get; init; }
    public string Name { get; init; } = "Unnamed Drone";
    [Range(0.0f, 100.0f)] public double BatteryPercent { get; private set; } = 100.0;
    public bool IsAirborne { get; protected set; } = false;

    /// <summary>
    /// Drains battery by the specified amount. Clamps to 0 if drain exceeds current battery.
    /// </summary>
    /// <param name="amount">The amount to drain from battery.</param>
    protected void DrainBattery(double amount)
    {
        BatteryPercent -= amount;
        if (BatteryPercent < 0)
        {
            BatteryPercent = 0;
        }
    }

    /// <inheritdoc/>
    public virtual void Land()
    {
        IsAirborne = false;
    }

    /// <inheritdoc/>
    public bool RunSelfTest()
    {
        return BatteryPercent >= 20.0f;
    }

    /// <inheritdoc/>
    public virtual void TakeOff()
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
        DrainBattery(5.0f);
    }

    /// <inheritdoc/>
    public void ChargeBattery(double charge)
    {
        if (charge < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(charge), "Charge amount cannot be negative.");
        }

        BatteryPercent += charge;

        if (BatteryPercent > 100.0f)
        {
            BatteryPercent = 100.0f;
        }
    }

    public override string ToString()
    {
        return $"ID: {this.Id}," +
            $" Name: {this.Name}," +
            $" Battery: {this.BatteryPercent}%," +
            $" Airborne: {this.IsAirborne}";
    }
}
