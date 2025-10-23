using System.Linq;
using DroneFleet.App.ConsoleApp;
using DroneFleet.Domain.Extensions;
using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Lists drones stored in the repository.
/// </summary>
internal sealed class ListCommand : IConsoleCommand
{
    public string Name => "list";

    public string Description => "Lists drones (all, airborne, or by type).";

    public string Usage => "list all | list airborne | list by type <kind>";

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count == 0)
        {
            context.WriteInfo("Usage: " + Usage);
            return ValueTask.CompletedTask;
        }

        IReadOnlyCollection<Drone> results;
        var mode = arguments[0].ToLowerInvariant();

        switch (mode)
        {
            case "all":
                results = context.FleetService.ListAll();
                break;
            case "airborne":
                results = context.FleetService.ListAirborne();
                break;
            case "by":
                results = HandleBy(context, arguments);
                break;
            case "type":
                results = HandleType(context, arguments.Skip(1).ToArray());
                break;
            default:
                context.WriteWarning("Unknown list option. " + Usage);
                return ValueTask.CompletedTask;
        }

        if (results.Count == 0)
        {
            context.WriteWarning("No drones found.");
            return ValueTask.CompletedTask;
        }

        foreach (var drone in results)
        {
            context.Output.WriteLine(drone.ToDisplayString());
        }

        return ValueTask.CompletedTask;
    }

    private static IReadOnlyCollection<Drone> HandleBy(CommandContext context, IReadOnlyList<string> args)
    {
        if (args.Count < 3 || !string.Equals(args[1], "type", StringComparison.OrdinalIgnoreCase))
        {
            context.WriteInfo("Usage: list by type <kind>");
            return Array.Empty<Drone>();
        }

        return HandleType(context, args.Skip(2).ToArray());
    }

    private static IReadOnlyCollection<Drone> HandleType(CommandContext context, IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            context.WriteInfo("Usage: list by type <kind>");
            return Array.Empty<Drone>();
        }

        if (!TryParseKind(args[0], out var kind))
        {
            context.WriteError("Unknown drone type. Supported values: delivery, survey, racing.");
            return Array.Empty<Drone>();
        }

        return context.FleetService.ListByKind(kind);
    }

    private static bool TryParseKind(string value, out DroneKind kind)
    {
        return Enum.TryParse(value, true, out kind);
    }
}
