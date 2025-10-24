using DroneFleet.Domain.Common;
using DroneFleet.Domain.Extensions;
using DroneFleet.Domain.Models;
using System.Text;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Displays aggregate statistics for the drone fleet.
/// </summary>
internal sealed class StatsCommand : IConsoleCommand
{
    public string Name => "stats";

    public string Description => "Shows fleet statistics.";

    public string Usage => "stats all | stats battery [topCount] | stats battery-below <percent> | stats cargo-remaining | stats kind | stats airborne | stats grounded";

    public string HelpText =>
        "stats all                      - Full summary" + Environment.NewLine +
        "stats battery [n]              - Top n battery levels (default 3)" + Environment.NewLine +
        "stats battery-below <percent>  - Drones with battery below threshold" + Environment.NewLine +
        "stats cargo-remaining          - Delivery drones with remaining capacity" + Environment.NewLine +
        "stats kind                     - Count by drone kind" + Environment.NewLine +
        "stats airborne                 - Airborne drone count & list" + Environment.NewLine +
        "stats grounded                 - Grounded drone count & list" + Environment.NewLine +
        "Examples:" + Environment.NewLine +
        "  stats battery 5" + Environment.NewLine +
        "  stats battery-below 25" + Environment.NewLine +
        "  stats cargo-remaining";

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var all = context.FleetService.ListAll();
        if (arguments.Count == 0)
        {
            WriteFullSummary(context, all);
            return ValueTask.CompletedTask;
        }

        var mode = arguments[0].ToLowerInvariant();
        return mode switch
        {
            "all" => WriteAndReturn(context, () => WriteFullSummary(context, all)),
            "battery" => WriteAndReturn(context, () => WriteBattery(context, all, arguments.Skip(1).ToArray())),
            "battery-below" => WriteAndReturn(context, () => WriteBatteryBelow(context, all, arguments.Skip(1).ToArray())),
            "cargo-remaining" => WriteAndReturn(context, () => WriteCargoRemaining(context, all)),
            "kind" => WriteAndReturn(context, () => WriteKindBreakdown(context, all)),
            "airborne" => WriteAndReturn(context, () => WriteAirborne(context, all)),
            "grounded" => WriteAndReturn(context, () => WriteGrounded(context, all)),
            _ => HandleUnknown(context)
        };
    }

    private static ValueTask WriteAndReturn(CommandContext ctx, Action action)
    {
        action();
        return ValueTask.CompletedTask;
    }

    private static ValueTask HandleUnknown(CommandContext context)
    {
        context.WriteError(ConsoleHttpStatusFormatter.Format(Result.Failure("Unknown stats mode.", ResultCodes.Validation), "Usage: stats all | stats battery [n] | stats battery-below <percent> | stats cargo-remaining | stats kind | stats airborne | stats grounded"));
        return ValueTask.CompletedTask;
    }

    private static void WriteFullSummary(CommandContext context, IReadOnlyCollection<Drone> drones)
    {
        var summary = drones.ToFleetSummary();
        var sb = new StringBuilder();
        sb.AppendLine(ConsoleHttpStatusFormatter.Format(Result.Success(), "Fleet statistics:"));
        sb.AppendLine($"  Total drones: {summary.TotalDrones}");
        sb.AppendLine($"  Airborne: {summary.AirborneDrones}");
        sb.AppendLine($"  Average battery: {summary.AverageBatteryPercent}%");
        sb.AppendLine($"  Total cargo load: {summary.TotalCargoLoadKg} kg");
        if (summary.DronesByKind.Count > 0)
        {
            sb.AppendLine("  Drones by type:");
            foreach (var entry in summary.DronesByKind)
            {
                sb.AppendLine($"    {entry.Kind}: {entry.Count}");
            }
        }
        if (summary.TopBatteryLevels.Count > 0)
        {
            sb.AppendLine("  Top battery levels:");
            foreach (var level in summary.TopBatteryLevels)
            {
                sb.AppendLine($"    {level}%");
            }
        }
        context.Output.WriteSuccessLine(sb.ToString().TrimEnd());
    }

    private static void WriteBattery(CommandContext context, IReadOnlyCollection<Drone> drones, IReadOnlyList<string> args)
    {
        int top = 3;
        if (args.Count > 0 && !int.TryParse(args[0], out top))
        {
            context.WriteError(ConsoleHttpStatusFormatter.Format(Result.Failure("Invalid top count.", ResultCodes.Validation), "Usage: stats battery [topCount]"));
            return;
        }
        if (top <= 0)
        {
            context.WriteError(ConsoleHttpStatusFormatter.Format(Result.Failure("Top count must be positive.", ResultCodes.Validation)));
            return;
        }
        var topDrones = drones.TopByBattery(top).ToArray();
        if (topDrones.Length == 0)
        {
            context.WriteWarning("No drones available.");
            return;
        }
        var sb = new StringBuilder();
        sb.AppendLine(ConsoleHttpStatusFormatter.Format(Result.Success(), $"Top {top} battery levels:"));
        foreach (var d in topDrones)
        {
            sb.AppendLine($"  #{d.Id} {d.Name} {d.BatteryPercent}% ({d.Kind})");
        }
        context.Output.WriteSuccessLine(sb.ToString().TrimEnd());
    }

    private static void WriteBatteryBelow(CommandContext context, IReadOnlyCollection<Drone> drones, IReadOnlyList<string> args)
    {
        if (args.Count == 0 || !double.TryParse(args[0], out var threshold))
        {
            context.WriteError(ConsoleHttpStatusFormatter.Format(Result.Failure("Missing/invalid threshold.", ResultCodes.Validation), "Usage: stats battery-below <percent>"));
            return;
        }
        var low = drones.WithBatteryBelow(threshold).OrderBy(d => d.BatteryPercent).ToArray();
        var sb = new StringBuilder();
        sb.AppendLine(ConsoleHttpStatusFormatter.Format(Result.Success(), $"Drones with battery below {threshold}% ({low.Length}):"));
        foreach (var d in low)
        {
            sb.AppendLine($"  #{d.Id} {d.Name} {d.BatteryPercent}% ({d.Kind})");
        }
        var output = sb.ToString().TrimEnd();
        context.Output.WriteSuccessLine(output);
    }

    private static void WriteCargoRemaining(CommandContext context, IReadOnlyCollection<Drone> drones)
    {
        var available = drones.WithCargoCapacityRemaining().OrderBy(d => d.Id).ToArray();
        var sb = new StringBuilder();
        sb.AppendLine(ConsoleHttpStatusFormatter.Format(Result.Success(), $"Delivery drones with remaining capacity ({available.Length}):"));
        foreach (var d in available)
        {
            sb.AppendLine($"  #{d.Id} {d.Name} Load {d.CurrentLoadKg}/{d.CapacityKg} kg ({Math.Round((d.CurrentLoadKg / d.CapacityKg) * 100, 2)}%)");
        }
        var output = sb.ToString().TrimEnd();
        context.Output.WriteSuccessLine(output);
    }

    private static void WriteKindBreakdown(CommandContext context, IReadOnlyCollection<Drone> drones)
    {
        var byKind = drones.GroupBy(d => d.Kind).Select(g => new { Kind = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).ToArray();
        var sb = new StringBuilder();
        sb.AppendLine(ConsoleHttpStatusFormatter.Format(Result.Success(), "Drones by type:"));
        foreach (var entry in byKind)
        {
            sb.AppendLine($"  {entry.Kind}: {entry.Count}");
        }
        context.Output.WriteSuccessLine(sb.ToString().TrimEnd());
    }

    private static void WriteAirborne(CommandContext context, IReadOnlyCollection<Drone> drones)
    {
        var airborne = drones.Airborne().OrderByDescending(d => d.BatteryPercent).ToArray();
        var sb = new StringBuilder();
        sb.AppendLine(ConsoleHttpStatusFormatter.Format(Result.Success(), $"Airborne drones ({airborne.Length}):"));
        foreach (var d in airborne)
        {
            sb.AppendLine($"  #{d.Id} {d.Name} Battery {d.BatteryPercent}%");
        }
        context.Output.WriteSuccessLine(sb.ToString().TrimEnd());
    }

    private static void WriteGrounded(CommandContext context, IReadOnlyCollection<Drone> drones)
    {
        var grounded = drones.Grounded().OrderBy(d => d.Id).ToArray();
        var sb = new StringBuilder();
        sb.AppendLine(ConsoleHttpStatusFormatter.Format(Result.Success(), $"Grounded drones ({grounded.Length}):"));
        foreach (var d in grounded)
        {
            sb.AppendLine($"  #{d.Id} {d.Name} Battery {d.BatteryPercent}%");
        }
        context.Output.WriteSuccessLine(sb.ToString().TrimEnd());
    }
}
