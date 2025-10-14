using DroneFleet.Models.Interfaces;

namespace DroneFleet.Models;

internal class SurveyDrone : Drone, INavigable, IPhotoCapture
{
    public (double lat, double lon)? CurrentWaypoint { get; private set; }

    public int PhotoCount { get; private set; } = 0;

    /// <inheritdoc/>
    public void SetWaypoint(double lat, double lon)
    {
        CurrentWaypoint = (lat, lon);
        DrainBattery(2.0f);
    }

    /// <inheritdoc/>
    public void TakePhoto()
    {
        if(!IsAirborne)
        {
            throw new InvalidOperationException("Drone must be airborne to take photos.");
        }

        ++PhotoCount;
        DrainBattery(1.0f);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return base.ToString() + ","+
            $" Waypoint: {(this.CurrentWaypoint != null ? this.CurrentWaypoint : "None")}," +
            $" PhotoCount: {this.PhotoCount}";
    }
}
