using DroneFleet.Domain.Contracts;
using DroneFleet.Domain.Models;

namespace DroneFleet.Infrastructure.Services.Interfaces;

public interface IDroneFactory
{
    /// <summary>
    /// Creates a new instance of a drone of the specified type <typeparamref name="TDrone"/>, 
    /// assigning the provided name and applying optional creation options.
    /// </summary>
    /// <typeparam name="TDrone">The type of drone to instantiate. Must inherit from <see cref="Drone"/>.</typeparam>
    /// <param name="name">The name to assign to the drone.</param>
    /// <param name="options">Optional parameters for drone creation, such as payload capacity.</param>
    /// <returns>A new instance of <typeparamref name="TDrone"/> initialized with the specified parameters.</returns>
    Drone Create<TDrone>(string name, DroneCreationOptions? options = null)
            where TDrone : Drone;
}
