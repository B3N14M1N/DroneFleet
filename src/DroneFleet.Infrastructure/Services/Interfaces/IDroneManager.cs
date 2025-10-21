using DroneFleet.Domain.Contracts;
using DroneFleet.Domain.Models;

namespace DroneFleet.Infrastructure.Services.Interfaces;

public interface IDroneManager
{
    /// <summary>
    /// Gets the collection of all managed drones.
    /// </summary>
    IEnumerable<Drone> Drones { get; }

    /// <summary>
    /// Adds a new drone to the fleet.
    /// </summary>
    /// <param name="drone">The drone instance to add.</param>
    void AddDrone(Drone drone);

    /// <summary>
    /// Charges the battery of all drones by the specified amount.
    /// </summary>
    /// <param name="chargeAmount">The amount to charge each drone's battery.</param>
    /// <returns>Collection describing the outcome for each drone.</returns>
    IEnumerable<DroneChargeResult> ChargeAllDrones(double chargeAmount);

    /// <summary>
    /// Retrieves a drone by its unique identifier.
    /// </summary>
    /// <param name="id">The unique ID of the drone.</param>
    /// <returns>The drone with the specified ID, or null if not found.</returns>
    Drone? GetDroneById(int id);

    /// <summary>
    /// Performs a pre-flight check on all drones.
    /// </summary>
    /// <returns>Collection describing the self-test outcome for each drone.</returns>
    IEnumerable<DroneSelfTestResult> PreFlightCheckAll();

    /// <summary>
    /// Removes a drone from the fleet by its unique identifier.
    /// </summary>
    /// <param name="id">The unique ID of the drone to remove.</param>
    /// <returns>True if the drone was removed; otherwise, false.</returns>
    bool RemoveDrone(int id);
}