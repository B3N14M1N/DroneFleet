using DroneFleet.Domain.Models;

namespace DroneFleet.Domain.Repositories;

/// <summary>
/// Defines the operations required for persisting drone entities.
/// </summary>
public interface IDroneRepository : IRepository<Drone, int>
{
    /// <summary>
    /// Provides a new unique identifier for a drone.
    /// </summary>
    int NextId();
}
