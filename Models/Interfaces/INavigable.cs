namespace DroneFleet.Models.Interfaces;

internal interface INavigable
{
    /// <summary>
    /// Sets the drone's next waypoint using latitude and longitude.
    /// </summary>
    /// <param name="lat">Latitude of the waypoint.</param>
    /// <param name="lon">Longitude of the waypoint.</param>
    void SetWaypoint(double lat, double lon);

    /// <summary>
    /// Gets the current waypoint coordinates, if set.
    /// </summary>
    (double lat, double lon)? CurrentWaypoint { get; }
}
