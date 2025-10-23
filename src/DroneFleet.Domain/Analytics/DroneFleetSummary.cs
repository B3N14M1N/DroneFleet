using DroneFleet.Domain.Models;

namespace DroneFleet.Domain.Analytics;

/// <summary>
/// Represents a snapshot of computed metrics over a fleet of drones.
/// </summary>
public sealed record DroneFleetSummary(
    int TotalDrones,
    int AirborneDrones,
    double AverageBatteryPercent,
    double TotalCargoLoadKg,
    IReadOnlyList<DroneKindBreakdown> DronesByKind,
    IReadOnlyList<double> TopBatteryLevels);

/// <summary>
/// Represents a count grouped by drone kind.
/// </summary>
public sealed record DroneKindBreakdown(DroneKind Kind, int Count);
