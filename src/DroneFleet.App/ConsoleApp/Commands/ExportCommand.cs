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

    public string HelpText =>
        "export json <path> | export csv <path>" + Environment.NewLine +
        "Exports fleet data. Relative paths are resolved against the project root." + Environment.NewLine +
        "Example: export csv data/out/fleet.csv";

    public async ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count < 2)
        {
            // Plain usage (no status codes) for simplicity
            context.WriteInfo("Usage: " + Usage);
            return;
        }

        var format = arguments[0].ToLowerInvariant();
        var destination = arguments[1];

        if (File.Exists(destination))
        {
            if (!await ConfirmOverwriteAsync(context, destination, cancellationToken))
            {
                // Simple warning without HTTP code noise
                context.WriteWarning("Export cancelled (overwrite declined).");
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
            var baseResult = Result.Failure(result.Value.Error ?? "Export failed.", result.Value.ErrorCode ?? ResultCodes.Validation);
            context.WriteError(ConsoleHttpStatusFormatter.Format(baseResult));
            return;
        }

        // Single success status-coded line
        context.WriteSuccess(ConsoleHttpStatusFormatter.Format(Result.Success(), $"Exported fleet to {destination}."));
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
        context.WriteError(ConsoleHttpStatusFormatter.Format(Result.Failure("Unknown export format.", ResultCodes.Validation), "Unknown export format. Use 'json' or 'csv'."));
        return null;
    }
}
