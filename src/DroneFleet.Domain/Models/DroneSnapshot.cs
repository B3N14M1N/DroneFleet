namespace DroneFleet.Domain.Models;

/// <summary>
/// Represents a serialisable drone snapshot used for import and export operations.
/// </summary>
public sealed record DroneSnapshot(
    int Id,
    string Name,
    DroneKind Kind,
    double BatteryPercent,
    bool IsAirborne,
    double? LoadKg,
    double? WaypointLat,
    double? WaypointLon,
    int? PhotoCount);
