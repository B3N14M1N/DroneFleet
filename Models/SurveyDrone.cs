using DroneFleet.Models.Interfaces;

namespace DroneFleet.Models;

internal class SurveyDrone : Drone, INavigable, IPhotoCapture
{
    private const double NAVIGATION_DRAIN = 2.0f;
    private const double PHOTO_DRAIN = 1.0f;

    public (double lat, double lon)? CurrentWaypoint { get; private set; }

    public int PhotoCount { get; private set; } = 0;

    /// <inheritdoc/>
    public void SetWaypoint(double lat, double lon)
    {
        if (BatteryPercent < NAVIGATION_DRAIN)
        {
            throw new InvalidOperationException($"Insufficient battery for movement." +
                $" Minimum {NAVIGATION_DRAIN}% required.");
        }

        CurrentWaypoint = (lat, lon);
        DrainBattery(NAVIGATION_DRAIN);
    }

    /// <inheritdoc/>
    public void TakePhoto()
    {
        if(!IsAirborne)
        {
            throw new InvalidOperationException("Drone must be airborne to take photos.");
        }

        if (BatteryPercent < PHOTO_DRAIN)
        {
            throw new InvalidOperationException($"Insufficient battery for survey." +
                $" Minimum {PHOTO_DRAIN}% required.");
        }

        ++PhotoCount;
        DrainBattery(PHOTO_DRAIN);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return base.ToString() + ","+
            $" Waypoint: {(this.CurrentWaypoint != null ? this.CurrentWaypoint : "None")}," +
            $" PhotoCount: {this.PhotoCount}";
    }
}
