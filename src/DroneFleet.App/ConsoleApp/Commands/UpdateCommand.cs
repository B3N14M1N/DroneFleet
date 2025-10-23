using System.Linq;
using DroneFleet.App.ConsoleApp;
using DroneFleet.App.ConsoleApp.Updates;
using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Updates drone properties.
/// </summary>
internal sealed class UpdateCommand(DroneUpdateRegistry registry) : IConsoleCommand
{
    private readonly DroneUpdateRegistry _registry = registry ?? throw new ArgumentNullException(nameof(registry));

    public string Name => "update";

    public string Description => "Updates drone attributes (battery, load, waypoint, etc).";

    public string Usage => "update <id> <action> [parameters]";

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count < 2)
        {
            context.WriteInfo("Usage: " + Usage);
            return ValueTask.CompletedTask;
        }

        if (!int.TryParse(arguments[0], out var id))
        {
            context.WriteError("Invalid id value.");
            return ValueTask.CompletedTask;
        }

        var action = arguments[1].ToLowerInvariant();
        var droneResult = context.FleetService.GetDrone(id);
        if (!droneResult.IsSuccess || droneResult.Value is null)
        {
            context.WriteError(droneResult.Error ?? "Drone not found.");
            return ValueTask.CompletedTask;
        }

        var drone = droneResult.Value;
        var handler = _registry.Resolve(action, drone);
        if (handler is null)
        {
            context.WriteWarning($"Unsupported update action '{action}' for {drone.Kind} drones.");
            var suggestions = _registry.GetSupportedKeywords(drone);
            if (suggestions.Count > 0)
            {
                context.WriteInfo("Available actions: " + string.Join(", ", suggestions));
            }

            return ValueTask.CompletedTask;
        }

        var updateArguments = arguments.Skip(2).ToArray();
        var response = handler.Execute(context, drone, updateArguments);

        if (!response.Result.IsSuccess)
        {
            context.WriteError(response.Result.Error ?? "Update failed.");
            return ValueTask.CompletedTask;
        }

        var message = string.IsNullOrWhiteSpace(response.SuccessMessage)
            ? $"Drone {drone.Id} updated."
            : response.SuccessMessage!;

        context.WriteSuccess(message);
        return ValueTask.CompletedTask;
    }
}
