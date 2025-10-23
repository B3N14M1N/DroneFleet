using System.IO;
using DroneFleet.App.ConsoleApp;
using DroneFleet.Domain.Common;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Exports the current fleet to CSV or JSON.
/// </summary>
internal sealed class ExportCommand : IConsoleCommand
{
    public string Name => "export";

    public string Description => "Exports the fleet to JSON or CSV.";

    public string Usage => "export json <path> | export csv <path>";

    public async ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count < 2)
        {
            context.WriteInfo("Usage: " + Usage);
            return;
        }

        var format = arguments[0].ToLowerInvariant();
        var destination = arguments[1];

        if (File.Exists(destination))
        {
            if (!await ConfirmOverwriteAsync(context, destination, cancellationToken))
            {
                context.WriteWarning("Export cancelled.");
                return;
            }
        }

        var result = format switch
        {
            "json" => await context.FleetService.ExportToJsonAsync(destination, cancellationToken),
            "csv" => await context.FleetService.ExportToCsvAsync(destination, cancellationToken),
            _ => ResultForInvalidFormat(context)
        };

        if (result is null)
        {
            return;
        }

        if (!result.Value.IsSuccess)
        {
            context.WriteError(result.Value.Error ?? "Export failed.");
            return;
        }

        context.WriteSuccess($"Exported fleet to {destination}.");
    }

    private static async Task<bool> ConfirmOverwriteAsync(CommandContext context, string destination, CancellationToken cancellationToken)
    {
        context.Output.Write($"File '{destination}' already exists. Overwrite? (y/n): ");
        var response = await context.Input.ReadLineAsync();
        cancellationToken.ThrowIfCancellationRequested();
        if (response is null)
        {
            return false;
        }

        var value = response.Trim().ToLowerInvariant();
        return value is "y" or "yes";
    }

    private static Result? ResultForInvalidFormat(CommandContext context)
    {
        context.WriteError("Unknown export format. Use 'json' or 'csv'.");
        return null;
    }
}
