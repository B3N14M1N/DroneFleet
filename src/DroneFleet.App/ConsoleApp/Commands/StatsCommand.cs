using DroneFleet.App.ConsoleApp;
using DroneFleet.Domain.Analytics;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Displays aggregate statistics for the drone fleet.
/// </summary>
internal sealed class StatsCommand : IConsoleCommand
{
    public string Name => "stats";

    public string Description => "Shows fleet statistics.";

    public string Usage => "stats";

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var summary = context.FleetService.GetSummary();
        WriteSummary(context, summary);
        return ValueTask.CompletedTask;
    }

    private static void WriteSummary(CommandContext context, DroneFleetSummary summary)
    {
        context.Output.WriteInfoLine($"Total drones: {summary.TotalDrones}");
        context.Output.WriteInfoLine($"Airborne: {summary.AirborneDrones}");
        context.Output.WriteInfoLine($"Average battery: {summary.AverageBatteryPercent}%");
        context.Output.WriteInfoLine($"Total cargo load: {summary.TotalCargoLoadKg} kg");

        if (summary.DronesByKind.Count > 0)
        {
            context.Output.WriteInfoLine("Drones by type:");
            foreach (var entry in summary.DronesByKind)
            {
                context.Output.WriteInfoLine($"  {entry.Kind}: {entry.Count}");
            }
        }

        if (summary.TopBatteryLevels.Count > 0)
        {
            context.Output.WriteInfoLine("Top battery levels:");
            foreach (var level in summary.TopBatteryLevels)
            {
                context.Output.WriteInfoLine($"  {level}%");
            }
        }
    }
}
