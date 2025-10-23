using DroneFleet.Domain.Analytics;
using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;
using DroneFleet.Domain.Operations;

namespace DroneFleet.Domain.Services;

/// <summary>
/// Describes the operations exposed to the application layer for managing a drone fleet.
/// </summary>
public interface IDroneFleetService
{
    /// <summary>
    /// Imports fleet information from the provided CSV files.
    /// </summary>
    Task<Result<FleetImportResult>> ImportFromCsvAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all drones stored in the repository.
    /// </summary>
    IReadOnlyCollection<Drone> ListAll();

    /// <summary>
    /// Retrieves a single drone by id.
    /// </summary>
    Result<Drone> GetDrone(int droneId);

    /// <summary>
    /// Retrieves all airborne drones.
    /// </summary>
    IReadOnlyCollection<Drone> ListAirborne();

    /// <summary>
    /// Retrieves drones filtered by the specified kind.
    /// </summary>
    IReadOnlyCollection<Drone> ListByKind(DroneKind kind);

    /// <summary>
    /// Charges the drone identified by the provided id by the specified percent.
    /// </summary>
    Result ChargeDrone(int droneId, double percent);

    /// <summary>
    /// Sets the battery level of the drone identified by the provided id.
    /// </summary>
    Result UpdateBattery(int droneId, double percent);

    /// <summary>
    /// Initiates take-off for the specified drone.
    /// </summary>
    Result TakeOff(int droneId);

    /// <summary>
    /// Initiates landing for the specified drone.
    /// </summary>
    Result Land(int droneId);

    /// <summary>
    /// Updates the waypoint for the specified drone when supported.
    /// </summary>
    Result SetWaypoint(int droneId, double latitude, double longitude);

    /// <summary>
    /// Updates the cargo load for a delivery drone.
    /// </summary>
    Result UpdateCargoLoad(int droneId, double kilograms);

    /// <summary>
    /// Clears the cargo load for a delivery drone.
    /// </summary>
    Result UnloadCargo(int droneId);

    /// <summary>
    /// Captures a photo using a survey drone.
    /// </summary>
    Result CapturePhoto(int droneId);

    /// <summary>
    /// Calculates a summary snapshot across the entire fleet.
    /// </summary>
    DroneFleetSummary GetSummary();

    /// <summary>
    /// Exports the fleet to a JSON file.
    /// </summary>
    Task<Result> ExportToJsonAsync(string destinationPath, CancellationToken cancellationToken);

    /// <summary>
    /// Exports the fleet to a CSV file.
    /// </summary>
    Task<Result> ExportToCsvAsync(string destinationPath, CancellationToken cancellationToken);
}
