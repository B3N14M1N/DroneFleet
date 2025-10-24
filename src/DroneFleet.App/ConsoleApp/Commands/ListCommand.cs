using DroneFleet.Domain.Common;
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

    public string Usage => "list all | list airborne | list by type <kind> | list id <id> | list ids <id1,id2,...>";

    public string HelpText =>
        "list all" + Environment.NewLine +
        "Lists every drone." + Environment.NewLine +
        "list airborne" + Environment.NewLine +
        "Lists drones currently airborne." + Environment.NewLine +
        "list by type <kind>" + Environment.NewLine +
        "Filters by delivery | survey | racing." + Environment.NewLine +
        "list id <id>" + Environment.NewLine +
        "Displays a single drone." + Environment.NewLine +
        "list ids <id1,id2,...>" + Environment.NewLine +
        "Lists specific drones by id (comma or space separated).";

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count == 0)
        {
            // Plain usage (no status code) for simplicity
            context.WriteInfo("Usage: " + Usage);
            return ValueTask.CompletedTask;
        }

        IReadOnlyCollection<Drone>? results;
        var mode = arguments[0].ToLowerInvariant();

        results = mode switch
        {
            "all" => context.FleetService.ListAll(),
            "airborne" => context.FleetService.ListAirborne(),
            "by" => HandleBy(context, arguments),
            "type" => HandleType(context, arguments.Skip(1).ToArray()),
            "id" => HandleSingleId(context, arguments.Skip(1).ToArray()),
            "ids" => HandleMultipleIds(context, arguments.Skip(1).ToArray()),
            _ => HandleUnknown(context)
        };

        if (results == null)
        {
            return ValueTask.CompletedTask; // unknown handled
        }

        if (results.Count == 0)
        {
            // Simple empty result notice (no status code noise)
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
            context.WriteError(ConsoleHttpStatusFormatter.Format(Result.Failure("Unknown drone type.", ResultCodes.Validation), "Unknown drone type. Supported values: delivery, survey, racing."));
            return Array.Empty<Drone>();
        }

        return context.FleetService.ListByKind(kind);
    }

    private static IReadOnlyCollection<Drone> HandleSingleId(CommandContext context, IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            context.WriteInfo(ConsoleHttpStatusFormatter.Format(Result.Failure("Missing id value.", ResultCodes.Validation), "Usage: list id <id>"));
            return Array.Empty<Drone>();
        }

        if (!int.TryParse(args[0], out var id))
        {
            context.WriteError(ConsoleHttpStatusFormatter.Format(Result.Failure("Invalid id value.", ResultCodes.Validation)));
            return Array.Empty<Drone>();
        }

        var result = context.FleetService.GetDrone(id);
        if (!result.IsSuccess || result.Value is null)
        {
            var baseResult = Result.Failure(result.Error ?? "Drone not found.", ResultCodes.NotFound);
            context.WriteWarning(ConsoleHttpStatusFormatter.Format(baseResult));
            return Array.Empty<Drone>();
        }

        return new[] { result.Value };
    }

    private static IReadOnlyCollection<Drone> HandleMultipleIds(CommandContext context, IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            context.WriteInfo("Usage: list ids <id1,id2,...>");
            return Array.Empty<Drone>();
        }

        var joined = string.Join(" ", args);
        var tokens = joined.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var ids = new List<int>();
        foreach (var token in tokens)
        {
            if (int.TryParse(token, out var id))
            {
                ids.Add(id);
            }
            else
            {
                context.WriteWarning(ConsoleHttpStatusFormatter.Format(Result.Failure("Invalid id token.", ResultCodes.Validation), $"Skipping invalid id '{token}'."));
            }
        }

        if (ids.Count == 0)
        {
            context.WriteWarning(ConsoleHttpStatusFormatter.Format(Result.Failure("No valid ids provided.", ResultCodes.Validation)));
            return Array.Empty<Drone>();
        }

        var results = new List<Drone>();
        foreach (var id in ids.Distinct())
        {
            var droneResult = context.FleetService.GetDrone(id);
            if (droneResult.IsSuccess && droneResult.Value is not null)
            {
                results.Add(droneResult.Value);
            }
            else
            {
                context.WriteWarning(ConsoleHttpStatusFormatter.Format(Result.Failure("Drone not found.", ResultCodes.NotFound), $"Drone {id} not found."));
            }
        }

        return results;
    }

    private static IReadOnlyCollection<Drone>? HandleUnknown(CommandContext context)
    {
        context.WriteError(ConsoleHttpStatusFormatter.Format(Result.Failure("Unknown list option.", ResultCodes.Validation), "Unknown list option. list all | list airborne | list by type <kind> | list id <id> | list ids <id1,id2,...>"));
        return null;
    }


    private static bool TryParseKind(string value, out DroneKind kind)
    {
        return Enum.TryParse(value, true, out kind);
    }
}
