using DroneFleet.App.ConsoleApp;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Charges a drone by the specified percent.
/// </summary>
internal sealed class ChargeCommand : IConsoleCommand
{
    public string Name => "charge";

    public string Description => "Charges the specified drone by the provided percentage.";

    public string Usage => "charge <id> <percent>";

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count != 2)
        {
            context.WriteInfo("Usage: " + Usage);
            return ValueTask.CompletedTask;
        }

        if (!int.TryParse(arguments[0], out var id))
        {
            context.WriteError("Invalid id value.");
            return ValueTask.CompletedTask;
        }

        if (!double.TryParse(arguments[1], out var percent))
        {
            context.WriteError("Invalid percent value.");
            return ValueTask.CompletedTask;
        }

        var result = context.FleetService.ChargeDrone(id, percent);
        if (!result.IsSuccess)
        {
            context.WriteError(result.Error ?? "Unable to charge drone.");
            return ValueTask.CompletedTask;
        }

        var updated = context.FleetService.GetDrone(id);
        if (updated.IsSuccess && updated.Value is not null)
        {
            context.WriteSuccess($"Drone {id} charged. Battery level: {updated.Value.BatteryPercent}%.");
        }
        else
        {
            context.WriteSuccess($"Drone {id} charged.");
        }

        return ValueTask.CompletedTask;
    }
}
