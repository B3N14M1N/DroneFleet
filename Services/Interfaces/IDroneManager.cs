using DroneFleet.Models;

namespace DroneFleet.Services.Interfaces;

internal interface IDroneManager
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
    void ChargeAllDrones(double chargeAmount);

    /// <summary>
    /// Retrieves a drone by its unique identifier.
    /// </summary>
    /// <param name="id">The unique ID of the drone.</param>
    /// <returns>The drone with the specified ID, or null if not found.</returns>
    Drone? GetDroneById(int id);

    /// <summary>
    /// Gets all drones that support the specified capability interface.
    /// </summary>
    /// <typeparam name="TCapability">The capability interface type to filter drones by.</typeparam>
    /// <returns>An enumerable of drones implementing the specified capability.</returns>
    IEnumerable<TCapability> GetDronesByCapability<TCapability>();

    /// <summary>
    /// Displays a list of all drones in the fleet.
    /// </summary>
    void ListDrones();

    /// <summary>
    /// Lists drones grouped by categories defined in the provided configuration.
    /// </summary>
    /// <param name="categories">Dictionary where key is the category name and value is the drone type.</param>
    public void ListDronesByCategory(Dictionary<string, Type> categories);

    /// <summary>
    /// Performs a pre-flight check on all drones.
    /// </summary>
    void PreFlightCheckAll();

    /// <summary>
    /// Removes a drone from the fleet by its unique identifier.
    /// </summary>
    /// <param name="id">The unique ID of the drone to remove.</param>
    /// <returns>True if the drone was removed; otherwise, false.</returns>
    bool RemoveDrone(int id);
}