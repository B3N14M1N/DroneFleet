using DroneFleet.Domain.Models;

namespace DroneFleet.Infrastructure.Services.Interfaces;

public interface IDroneCreator
{
    /// <summary>
    /// The unique key to identify this type of drone creator.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// The display name for this type of drone creator.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Creates a new interactive drone instance by prompting for input, validating the provided data, and using the
    /// specified factory to construct the drone.
    /// </summary>
    /// <remarks>This method interacts with the user to gather the necessary input for creating the drone.
    /// Ensure that the factory provided is capable of handling the input and creating the desired drone type.</remarks>
    /// <param name="factory">The factory used to create the drone. Must implement <see cref="IDroneFactory"/>.</param>
    /// <returns>A newly created <see cref="Drone"/> instance based on the provided input and validation.</returns>
    Drone CreateInteractive(IDroneFactory factory);
}
