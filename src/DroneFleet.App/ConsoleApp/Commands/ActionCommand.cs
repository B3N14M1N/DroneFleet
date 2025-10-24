using DroneFleet.App.ConsoleApp.Updates;
using DroneFleet.Domain.Common;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Performs actions on a drone (charge, takeoff, waypoint, cargo, etc.).
/// </summary>
internal sealed class ActionCommand(DroneUpdateRegistry registry) : IConsoleCommand
{
    private readonly DroneUpdateRegistry _registry = registry ?? throw new ArgumentNullException(nameof(registry));

    public string Name => "action";

    public string Description => "Performs an action on a drone (charge, takeoff, land, waypoint, cargo, snap).";

    public string Usage => "action <id> <verb> [parameters]";

    public string HelpText => BuildHelpText();

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count < 2)
        {
            // Plain usage (no status code) for simplicity
            context.WriteInfo("Usage: " + Usage);
            return ValueTask.CompletedTask;
        }

        if (!int.TryParse(arguments[0], out var id))
        {
            var formatted = ConsoleHttpStatusFormatter.Format(Result.Failure("Invalid id value.", ResultCodes.Validation));
            context.WriteError(formatted);
            return ValueTask.CompletedTask;
        }

        var verb = arguments[1].ToLowerInvariant();
        var droneResult = context.FleetService.GetDrone(id);
        if (!droneResult.IsSuccess || droneResult.Value is null)
        {
            var formatted = ConsoleHttpStatusFormatter.Format(Result.Failure(droneResult.Error ?? "Drone not found.", ResultCodes.NotFound));
            context.WriteError(formatted);
            return ValueTask.CompletedTask;
        }

        var drone = droneResult.Value;
        var handler = _registry.Resolve(verb, drone);
        if (handler is null)
        {
            var formatted = ConsoleHttpStatusFormatter.Format(Result.Failure($"Unsupported action '{verb}' for {drone.Kind} drones.", ResultCodes.Validation));
            context.WriteWarning(formatted);
            var suggestions = _registry.GetSupportedKeywords(drone);
            if (suggestions.Count > 0)
            {
                // Provide suggestions without HTTP code noise
                context.WriteInfo("Available: " + string.Join(", ", suggestions));
            }

            return ValueTask.CompletedTask;
        }

        var actionArguments = arguments.Skip(2).ToArray();
        var response = handler.Execute(context, drone, actionArguments);

        if (!response.Result.IsSuccess)
        {
            var formatted = ConsoleHttpStatusFormatter.Format(response.Result);
            context.WriteError(formatted);
            return ValueTask.CompletedTask;
        }

        var message = string.IsNullOrWhiteSpace(response.SuccessMessage)
            ? $"Action completed for drone {drone.Id}."
            : response.SuccessMessage!;
        // Single success line with status code
        var formattedSuccess = ConsoleHttpStatusFormatter.Format(Result.Success(), message);
        context.WriteSuccess(formattedSuccess);
        return ValueTask.CompletedTask;
    }

    private string BuildHelpText()
    {
        var keywords = _registry.GetAllKeywords();
        var actions = string.Join(", ", keywords);
        return "action <id> <verb> [parameters]" + Environment.NewLine +
            "Verbs: " + actions + Environment.NewLine +
            "Examples:" + Environment.NewLine +
            "  action 1 charge 75" + Environment.NewLine +
            "  action 2 takeoff" + Environment.NewLine +
            "  action 3 waypoint 51.5 -0.12" + Environment.NewLine +
            "  action 4 cargo 3.5" + Environment.NewLine +
            "  action 4 unload" + Environment.NewLine +
            "  action 5 snap";
    }
}
