using DroneFleet.Domain.Analytics;
using DroneFleet.Domain.Models;

namespace DroneFleet.Domain.Extensions;

/// <summary>
/// Provides extension methods for working with drone collections.
/// </summary>
public static class DroneCollectionExtensions
{
    /// <summary>
    /// Calculates the average battery level of the provided drone sequence.
    /// </summary>
    public static double AverageBattery(this IEnumerable<Drone> drones)
    {
        ArgumentNullException.ThrowIfNull(drones);

        var materialised = drones as IList<Drone> ?? drones.ToList();
        if (materialised.Count == 0)
        {
            return 0;
        }

        return Math.Round(materialised.Average(d => d.BatteryPercent), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Calculates the total cargo load across the provided drone sequence.
    /// </summary>
    public static double TotalCargoLoad(this IEnumerable<Drone> drones)
    {
        ArgumentNullException.ThrowIfNull(drones);

        return Math.Round(drones.OfType<DeliveryDrone>().Sum(d => d.CurrentLoadKg), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Returns the top battery levels in descending order.
    /// </summary>
    public static IReadOnlyList<double> TopBatteryLevels(this IEnumerable<Drone> drones, int count)
    {
        ArgumentNullException.ThrowIfNull(drones);
        if (count <= 0)
        {
            return Array.Empty<double>();
        }

        return drones
            .Select(d => d.BatteryPercent)
            .OrderByDescending(b => b)
            .Take(count)
            .Select(b => Math.Round(b, 2, MidpointRounding.AwayFromZero))
            .ToArray();
    }

    /// <summary>
    /// Creates a display-friendly string describing a drone.
    /// </summary>
    public static string ToDisplayString(this Drone drone)
    {
        ArgumentNullException.ThrowIfNull(drone);
        return drone switch
        {
            DeliveryDrone delivery => $"[{delivery.Kind}] #{delivery.Id} {delivery.Name} | Battery {delivery.BatteryPercent}% | Airborne {delivery.IsAirborne} | Load {delivery.CurrentLoadKg}/{delivery.CapacityKg} kg",
            SurveyDrone survey => $"[{survey.Kind}] #{survey.Id} {survey.Name} | Battery {survey.BatteryPercent}% | Airborne {survey.IsAirborne} | PhotoCount {survey.PhotoCount}",
            _ => $"[{drone.Kind}] #{drone.Id} {drone.Name} | Battery {drone.BatteryPercent}% | Airborne {drone.IsAirborne}"
        };
    }

    /// <summary>
    /// Builds a summary of the fleet using the provided drones.
    /// </summary>
    public static DroneFleetSummary ToFleetSummary(this IEnumerable<Drone> drones, int topCount = 3)
    {
        ArgumentNullException.ThrowIfNull(drones);

        var snapshot = drones as IList<Drone> ?? drones.ToList();
        var total = snapshot.Count;
        var airborne = snapshot.Count(d => d.IsAirborne);
        var averageBattery = snapshot.AverageBattery();
        var cargoLoad = snapshot.TotalCargoLoad();

        var byKind = Enum
            .GetValues<DroneKind>()
            .Select(kind => new DroneKindBreakdown(kind, snapshot.Count(d => d.Kind == kind)))
            .Where(entry => entry.Count > 0)
            .ToArray();

        var topBatteries = snapshot.TopBatteryLevels(topCount);

        return new DroneFleetSummary(total, airborne, averageBattery, cargoLoad, byKind, topBatteries);
    }
}
