using DroneFleet.Domain.Common;

namespace DroneFleet.Domain.Models;

/// <summary>
/// Represents a drone dedicated to surveying and photo capture tasks.
/// </summary>
public sealed class SurveyDrone : Drone
{
    private const double NAVIGATION_DRAIN = 2.0f;
    private const double PHOTO_DRAIN = 1.0f;

    public SurveyDrone() : base(DroneKind.Survey)
    {
    }

    public (double lat, double lon)? CurrentWaypoint { get; private set; }

    public int PhotoCount { get; private set; } = 0;

    /// <summary>
    /// Applies a persisted snapshot to the survey drone without performing side effects.
    /// </summary>
    /// <param name="photoCount">The photo count recorded for the drone.</param>
    /// <param name="waypoint">The waypoint to restore.</param>
    internal void ApplySnapshot(int? photoCount, (double lat, double lon)? waypoint)
    {
        PhotoCount = Math.Max(0, photoCount ?? 0);
        CurrentWaypoint = waypoint;
    }

    /// <summary>
    /// Assigns a waypoint to the survey drone if sufficient battery remains.
    /// </summary>
    public Result SetWaypoint(double lat, double lon)
    {
        if (BatteryPercent < NAVIGATION_DRAIN)
        {
            return Result.Failure($"Insufficient battery for movement. Minimum {NAVIGATION_DRAIN}% required.", ResultCodes.Validation);
        }

        var drainResult = DrainBattery(NAVIGATION_DRAIN);
        if (!drainResult.IsSuccess)
        {
            return drainResult;
        }

        CurrentWaypoint = (lat, lon);
        return Result.Success();
    }

    /// <summary>
    /// Captures a photo when airborne and with sufficient battery.
    /// </summary>
    public Result TakePhoto()
    {
        if (!IsAirborne)
        {
            return Result.Failure("Drone must be airborne to take photos.", ResultCodes.Validation);
        }

        if (BatteryPercent < PHOTO_DRAIN)
        {
            return Result.Failure($"Insufficient battery for survey. Minimum {PHOTO_DRAIN}% required.", ResultCodes.Validation);
        }

        var drainResult = DrainBattery(PHOTO_DRAIN);
        if (!drainResult.IsSuccess)
        {
            return drainResult;
        }

        ++PhotoCount;
        return Result.Success();
    }

    public override string ToString()
    {
        return base.ToString() + "," +
            $" Waypoint: {(CurrentWaypoint != null ? CurrentWaypoint : "None")}," +
            $" PhotoCount: {PhotoCount}";
    }
}
