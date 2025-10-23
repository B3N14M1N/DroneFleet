using DroneFleet.App.ConsoleApp;
using DroneFleet.Domain.Common;
using DroneFleet.Domain.Operations;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Imports drone data from one or more CSV files.
/// </summary>
internal sealed class ImportCommand : IConsoleCommand
{
    public string Name => "import";

    public string Description => "Imports fleet data from CSV files.";

    public string Usage => "import <file1.csv> [file2.csv ...]";

    public async ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count == 0)
        {
            context.WriteInfo("Usage: " + Usage);
            return;
        }

        var result = await context.FleetService.ImportFromCsvAsync(arguments, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            context.WriteError(result.Error ?? "Import failed.");
            return;
        }

        WriteSummary(context, result.Value);
    }

    private static void WriteSummary(CommandContext context, FleetImportResult summary)
    {
        context.Output.WriteInfoLine($"Files processed: {summary.FilesProcessed}");
        context.Output.WriteInfoLine($"Rows processed: {summary.TotalRows}");
        context.Output.WriteSuccessLine($"Imported: {summary.Imported}");

        if (summary.Duplicates > 0)
        {
            context.Output.WriteWarningLine($"Duplicates skipped: {summary.Duplicates}");
        }
        else
        {
            context.Output.WriteInfoLine("Duplicates skipped: 0");
        }

        if (summary.Malformed > 0)
        {
            context.Output.WriteErrorLine($"Malformed rows: {summary.Malformed}");
        }
        else
        {
            context.Output.WriteInfoLine("Malformed rows: 0");
        }

        if (summary.Issues.Count > 0)
        {
            context.Output.WriteWarningLine("Issues:");
            foreach (var issue in summary.Issues)
            {
                context.Output.WriteWarningLine($"  {issue.Source}:{issue.LineNumber} - {issue.Message}");
            }
        }
        else
        {
            context.Output.WriteSuccessLine("No issues detected.");
        }
    }
}
