using DroneFleet.Domain.Common;
using DroneFleet.Domain.Mappers;
using DroneFleet.Domain.Models;
using DroneFleet.Domain.Operations;
using DroneFleet.Infrastructure.FileIO;
using System.Collections.Concurrent;
using System.Text.Json;

namespace DroneFleet.Infrastructure.Services;

public sealed partial class DroneFleetService
{
    public async Task<Result<FleetImportResult>> ImportAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filePaths);

        var candidates = GetCandidateRoots();
        var files = filePaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => ResolveExistingPath(path, candidates))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (files.Length == 0)
        {
            return Result<FleetImportResult>.Failure("No file paths were provided.", ResultCodes.Validation);
        }

        var issues = new ConcurrentBag<ImportIssue>();
        var filesProcessed = 0;
        long totalRows = 0;
        long imported = 0;
        long duplicates = 0;
        long malformed = 0;

        var tasks = files.Select(async filePath =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileName = Path.GetFileName(filePath);

            if (!File.Exists(filePath))
            {
                issues.Add(new ImportIssue(fileName, 0, "File not found."));
                Interlocked.Increment(ref filesProcessed);
                Interlocked.Increment(ref malformed);
                return;
            }

            Interlocked.Increment(ref filesProcessed);

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            try
            {
                switch (ext)
                {
                    case ".csv":
                        await ImportCsvFileAsync(filePath, fileName, cancellationToken);
                        break;
                    case ".json":
                        await ImportJsonFileAsync(filePath, fileName, cancellationToken);
                        break;
                    default:
                        issues.Add(new ImportIssue(fileName, 0, "Unsupported file type."));
                        Interlocked.Increment(ref malformed);
                        break;
                }
            }
            catch (Exception ex)
            {
                issues.Add(new ImportIssue(fileName, 0, $"Failed to read file. {ex.Message}"));
                Interlocked.Increment(ref malformed);
            }
        }).ToArray();

        // Local functions keep state updates atomic via Interlocked
        async Task ImportCsvFileAsync(string path, string fileName, CancellationToken ct)
        {
            var lines = await File.ReadAllLinesAsync(path, ct);
            if (lines.Length == 0)
            {
                issues.Add(new ImportIssue(fileName, 0, "File is empty."));
                Interlocked.Increment(ref malformed);
                return;
            }

            // Skip header (first line) if present
            var lineNumber = 1;
            foreach (var raw in lines.Skip(1))
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(raw)) continue;
                Interlocked.Increment(ref totalRows);

                var columns = CsvLineTokenizer.Tokenize(raw);
                var parseResult = _parser.Parse(columns);
                if (!parseResult.IsSuccess || parseResult.Value is null)
                {
                    Interlocked.Increment(ref malformed);
                    issues.Add(new ImportIssue(fileName, lineNumber, parseResult.Error ?? "Malformed row."));
                    continue;
                }

                var snapshot = parseResult.Value;
                var droneResult = snapshot.ToDrone();
                if (!droneResult.IsSuccess || droneResult.Value is null)
                {
                    Interlocked.Increment(ref malformed);
                    issues.Add(new ImportIssue(fileName, lineNumber, droneResult.Error ?? "Invalid drone snapshot."));
                    continue;
                }

                var addResult = _repository.Add(droneResult.Value);
                if (!addResult.IsSuccess)
                {
                    if (addResult.ErrorCode == ResultCodes.DuplicateKey)
                    {
                        Interlocked.Increment(ref duplicates);
                        issues.Add(new ImportIssue(fileName, lineNumber, addResult.Error ?? "Duplicate drone id."));
                    }
                    else
                    {
                        Interlocked.Increment(ref malformed);
                        issues.Add(new ImportIssue(fileName, lineNumber, addResult.Error ?? "Failed to store drone."));
                    }
                    continue;
                }
                Interlocked.Increment(ref imported);
            }
        }

        async Task ImportJsonFileAsync(string path, string fileName, CancellationToken ct)
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.Asynchronous | FileOptions.SequentialScan);
            var snapshots = await JsonSerializer.DeserializeAsync<DroneSnapshot[]>(stream, _jsonOptions, ct);
            if (snapshots is null || snapshots.Length == 0)
            {
                // Fallback: attempt to parse a single object
                stream.Position = 0;
                var single = await JsonSerializer.DeserializeAsync<DroneSnapshot>(stream, _jsonOptions, ct);
                if (single is null)
                {
                    issues.Add(new ImportIssue(fileName, 0, "JSON file is empty or invalid."));
                    Interlocked.Increment(ref malformed);
                    return;
                }
                snapshots = new[] { single };
            }
            var lineNumber = 0;
            foreach (var snapshot in snapshots)
            {
                ct.ThrowIfCancellationRequested();
                lineNumber++;
                Interlocked.Increment(ref totalRows);
                var droneResult = snapshot.ToDrone();
                if (!droneResult.IsSuccess || droneResult.Value is null)
                {
                    Interlocked.Increment(ref malformed);
                    issues.Add(new ImportIssue(fileName, lineNumber, droneResult.Error ?? "Invalid drone snapshot."));
                    continue;
                }
                var addResult = _repository.Add(droneResult.Value);
                if (!addResult.IsSuccess)
                {
                    if (addResult.ErrorCode == ResultCodes.DuplicateKey)
                    {
                        Interlocked.Increment(ref duplicates);
                        issues.Add(new ImportIssue(fileName, lineNumber, addResult.Error ?? "Duplicate drone id."));
                    }
                    else
                    {
                        Interlocked.Increment(ref malformed);
                        issues.Add(new ImportIssue(fileName, lineNumber, addResult.Error ?? "Failed to store drone."));
                    }
                    continue;
                }
                Interlocked.Increment(ref imported);
            }
        }

        await Task.WhenAll(tasks);

        var orderedIssues = issues
            .OrderBy(issue => issue.Source, StringComparer.OrdinalIgnoreCase)
            .ThenBy(issue => issue.LineNumber)
            .ToArray();

        var result = new FleetImportResult(
            filesProcessed,
            (int)Math.Min(int.MaxValue, totalRows),
            (int)Math.Min(int.MaxValue, imported),
            (int)Math.Min(int.MaxValue, duplicates),
            (int)Math.Min(int.MaxValue, malformed),
            orderedIssues);

        return Result<FleetImportResult>.Success(result);
    }
}
