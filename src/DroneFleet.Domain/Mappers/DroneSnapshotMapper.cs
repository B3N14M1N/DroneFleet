using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;

namespace DroneFleet.Domain.Mappers;

/// <summary>
/// Provides helper methods for converting between domain entities and snapshots.
/// </summary>
public static class DroneSnapshotMapper
{
    /// <summary>
    /// Creates a snapshot representation of the provided drone.
    /// </summary>
    public static DroneSnapshot ToSnapshot(this Drone drone)
    {
        ArgumentNullException.ThrowIfNull(drone);

        return drone switch
        {
            DeliveryDrone delivery => new DroneSnapshot(
                delivery.Id,
                delivery.Name,
                delivery.Kind,
                delivery.BatteryPercent,
                delivery.IsAirborne,
                delivery.CurrentLoadKg,
                delivery.CurrentWaypoint?.lat,
                delivery.CurrentWaypoint?.lon,
                null),
            SurveyDrone survey => new DroneSnapshot(
                survey.Id,
                survey.Name,
                survey.Kind,
                survey.BatteryPercent,
                survey.IsAirborne,
                null,
                survey.CurrentWaypoint?.lat,
                survey.CurrentWaypoint?.lon,
                survey.PhotoCount),
            _ => new DroneSnapshot(
                drone.Id,
                drone.Name,
                drone.Kind,
                drone.BatteryPercent,
                drone.IsAirborne,
                null,
                null,
                null,
                null)
        };
    }

    /// <summary>
    /// Builds a domain drone from the provided snapshot.
    /// </summary>
    public static Result<Drone> ToDrone(this DroneSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (string.IsNullOrWhiteSpace(snapshot.Name))
        {
            return Result<Drone>.Failure("Snapshot name cannot be empty.", ResultCodes.Validation);
        }

        if (snapshot.BatteryPercent is < 0 or > 100)
        {
            return Result<Drone>.Failure("Battery percent must be between 0 and 100.", ResultCodes.Validation);
        }

        Drone drone = snapshot.Kind switch
        {
            DroneKind.Delivery => CreateDelivery(snapshot),
            DroneKind.Survey => CreateSurvey(snapshot),
            DroneKind.Racing => CreateRacing(snapshot),
            _ => throw new ArgumentOutOfRangeException(nameof(snapshot.Kind), snapshot.Kind, "Unsupported drone kind.")
        };

        var telemetryResult = drone.ApplyTelemetry(snapshot.BatteryPercent, snapshot.IsAirborne);
        if (!telemetryResult.IsSuccess)
        {
            return Result<Drone>.Failure(telemetryResult.Error ?? "Invalid telemetry.", telemetryResult.ErrorCode);
        }

        return Result<Drone>.Success(drone);
    }

    private static Drone CreateDelivery(DroneSnapshot snapshot)
    {
        var drone = new DeliveryDrone()
        {
            Id = snapshot.Id,
            Name = snapshot.Name
        };

        drone.ApplySnapshot(snapshot.LoadKg, snapshot.WaypointLat.HasValue && snapshot.WaypointLon.HasValue
            ? (snapshot.WaypointLat.Value, snapshot.WaypointLon.Value)
            : null);

        return drone;
    }

    private static Drone CreateSurvey(DroneSnapshot snapshot)
    {
        var drone = new SurveyDrone()
        {
            Id = snapshot.Id,
            Name = snapshot.Name
        };

        drone.ApplySnapshot(snapshot.PhotoCount, snapshot.WaypointLat.HasValue && snapshot.WaypointLon.HasValue
            ? (snapshot.WaypointLat.Value, snapshot.WaypointLon.Value)
            : null);

        return drone;
    }

    private static Drone CreateRacing(DroneSnapshot snapshot)
    {
        return new RacingDrone()
        {
            Id = snapshot.Id,
            Name = snapshot.Name
        };
    }
}
