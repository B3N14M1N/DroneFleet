using DroneFleet.Models;

namespace DroneFleet.Services.Interfaces;

/// <summary>
/// Represents a menu action that can be performed for a drone capability.
/// </summary>
internal interface ICapabilityActionHandler
{
    /// <summary>
    /// Unique key that identifies the capability action.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Human-readable description presented to the user.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Determines whether this capability action supports the specified drone.
    /// </summary>
    /// <param name="drone">The drone to test for compatibility.</param>
    /// <returns>True if the action can be executed for the given drone; otherwise, false.</returns>
    bool Supports(Drone drone);

    /// <summary>
    /// Executes the capability action for the specified drone.
    /// </summary>
    /// <param name="drone">The drone on which to execute the action.</param>
    void Execute(Drone drone);
}
