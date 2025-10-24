using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Updates;

/// <summary>
/// Describes a handler that can process type-specific drone updates.
/// </summary>
internal interface IDroneUpdateHandler
{
    /// <summary>
    /// Gets the keyword associated with this handler.
    /// </summary>
    string Keyword { get; }

    /// <summary>
    /// Determines whether the handler supports the provided drone.
    /// </summary>
    bool Supports(Drone drone);

    /// <summary>
    /// Executes the update using the provided arguments.
    /// </summary>
    DroneUpdateResponse Execute(CommandContext context, Drone drone, IReadOnlyList<string> arguments);
}
