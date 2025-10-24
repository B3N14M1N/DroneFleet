using DroneFleet.Domain.Common;
using DroneFleet.Domain.Operations;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Imports drone data from one or more CSV files.
/// </summary>
internal sealed class ImportCommand : IConsoleCommand
{
    public string Name => "import";

    public string Description => "Imports fleet data from CSV or JSON files.";

    public string Usage => "import <file1.csv|file1.json> [file2.csv|file2.json ...]";

    public string HelpText =>
    "import <file1.csv|file1.json> [file2.csv|file2.json ...]" + Environment.NewLine +
    "Imports one or more CSV or JSON files. Relative paths are resolved against the project root." + Environment.NewLine +
    "CSV requires header: Id,Name,Kind,BatteryPercent,IsAirborne,LoadKg,WaypointLat,WaypointLon,PhotoCount" + Environment.NewLine +
    "JSON expects an array of snapshot objects with matching property names.";

    public async ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count == 0)
        {
            context.WriteInfo("Usage: " + Usage);
            return;
        }

    var result = await context.FleetService.ImportAsync(arguments, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            var baseResult = Result.Failure(result.Error ?? "Import failed.", result.ErrorCode ?? ResultCodes.Validation);
            context.WriteError(ConsoleHttpStatusFormatter.Format(baseResult));
            return;
        }

        WriteSummary(context, result.Value);
    }

    private static void WriteSummary(CommandContext context, FleetImportResult summary)
    {
        var header = new System.Text.StringBuilder();
        var issues = new System.Text.StringBuilder();

        // Header & counts (always success colour regardless of issues)
        header.AppendLine(ConsoleHttpStatusFormatter.Format(Result.Success(), "Import complete"));
        header.AppendLine($"  Files processed: {summary.FilesProcessed}");
        header.AppendLine($"  Rows processed: {summary.TotalRows}");
        header.AppendLine($"  Imported: {summary.Imported}");
        header.AppendLine(summary.Duplicates > 0
            ? $"  Duplicates skipped: {summary.Duplicates}"
            : "  Duplicates: 0");
        header.AppendLine(summary.Malformed > 0
            ? $"  Malformed rows: {summary.Malformed}"
            : "  Malformed rows: 0");

        // Issues block (each with code)
        if (summary.Issues.Count > 0)
        {
            foreach (var issue in summary.Issues)
            {
                var messageLower = issue.Message.ToLowerInvariant();
                Result lineResult = messageLower.Contains("duplicate")
                    ? Result.Failure(issue.Message, ResultCodes.DuplicateKey)
                    : messageLower.Contains("not found")
                        ? Result.Failure(issue.Message, ResultCodes.NotFound)
                        : Result.Failure(issue.Message, ResultCodes.Validation);

                var formatted = ConsoleHttpStatusFormatter.Format(lineResult, $"{issue.Source}:{issue.LineNumber} - {issue.Message}");
                issues.AppendLine(formatted);
            }
        }

        // Write header first (always green)
        context.Output.WriteSuccessLine(header.ToString().TrimEnd());

        if (issues.Length > 0)
        {
            // Classify overall severity of issues block
            bool hasErrors = summary.Issues.Any(i => !i.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
            bool hasDuplicatesOnly = summary.Issues.Count > 0 && !hasErrors;
            var issuesText = issues.ToString().TrimEnd();
            if (hasErrors)
                context.Output.WriteErrorLine(issuesText);
            else if (hasDuplicatesOnly)
                context.Output.WriteWarningLine(issuesText);
        }
    }
}
