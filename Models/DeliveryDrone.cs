using DroneFleet.Models.Interfaces;

namespace DroneFleet.Models;

internal class DeliveryDrone : Drone, INavigable, ICargoCarrier
{
    public (double lat, double lon)? CurrentWaypoint { get; private set; }

    public double CapacityKg { get; init; }

    public double CurrentLoadKg { get; private set; }

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
