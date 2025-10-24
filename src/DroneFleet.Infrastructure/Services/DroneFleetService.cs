using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using DroneFleet.Domain.Analytics;
using DroneFleet.Domain.Common;
using DroneFleet.Domain.Extensions;
using DroneFleet.Domain.Mappers;
using DroneFleet.Domain.Models;
using DroneFleet.Domain.Operations;
using DroneFleet.Domain.Repositories;
using DroneFleet.Domain.Services;
using DroneFleet.Infrastructure.FileIO;

namespace DroneFleet.Infrastructure.Services;

/// <summary>
/// Provides fleet management operations implemented against an in-memory repository.
/// </summary>
public sealed class DroneFleetService(IDroneRepository repository) : IDroneFleetService
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    private readonly IDroneRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly DroneCsvParser _parser = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task<Result<FleetImportResult>> ImportFromCsvAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken)
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

        await Parallel.ForEachAsync(files, cancellationToken, async (filePath, ct) =>
        {
            var fileName = Path.GetFileName(filePath);

            if (!File.Exists(filePath))
            {
                issues.Add(new ImportIssue(fileName, 0, "File not found."));
                Interlocked.Increment(ref filesProcessed);
                Interlocked.Increment(ref malformed);
                return;
            }

            Interlocked.Increment(ref filesProcessed);

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.Asynchronous | FileOptions.SequentialScan);
                using var reader = new StreamReader(stream);

                var header = await reader.ReadLineAsync();
                var lineNumber = 1;

                if (header is null)
                {
                    issues.Add(new ImportIssue(fileName, 0, "File is empty."));
                    return;
                }

                while (!reader.EndOfStream)
                {
                    ct.ThrowIfCancellationRequested();

                    var line = await reader.ReadLineAsync();
                    lineNumber++;

                    if (line is null)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    Interlocked.Increment(ref totalRows);

                    var columns = CsvLineTokenizer.Tokenize(line);
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
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                issues.Add(new ImportIssue(fileName, 0, $"Failed to read file. {ex.Message}"));
            }
        });

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

    public IReadOnlyCollection<Drone> ListAll()
    {
        return _repository.List()
            .OrderBy(drone => drone.Id)
            .ToArray();
    }

    public Result<Drone> GetDrone(int droneId)
    {
        return _repository.GetById(droneId);
    }

    public IReadOnlyCollection<Drone> ListAirborne()
    {
        return ListAll()
            .Where(drone => drone.IsAirborne)
            .ToArray();
    }

    public IReadOnlyCollection<Drone> ListByKind(DroneKind kind)
    {
        return ListAll()
            .Where(drone => drone.Kind == kind)
            .ToArray();
    }

    public Result ChargeDrone(int droneId, double percent)
    {
        var droneResult = _repository.GetById(droneId);
        if (!droneResult.IsSuccess || droneResult.Value is null)
        {
            return Result.Failure(droneResult.Error ?? "Drone not found.", droneResult.ErrorCode ?? ResultCodes.NotFound);
        }

        return droneResult.Value.ChargeBattery(percent);
    }

    public Result UpdateBattery(int droneId, double percent)
    {
        var droneResult = _repository.GetById(droneId);
        if (!droneResult.IsSuccess || droneResult.Value is null)
        {
            return Result.Failure(droneResult.Error ?? "Drone not found.", droneResult.ErrorCode ?? ResultCodes.NotFound);
        }

        return droneResult.Value.SetBatteryPercent(percent);
    }

    public Result TakeOff(int droneId)
    {
        var droneResult = _repository.GetById(droneId);
        if (!droneResult.IsSuccess || droneResult.Value is null)
        {
            return Result.Failure(droneResult.Error ?? "Drone not found.", droneResult.ErrorCode ?? ResultCodes.NotFound);
        }

        return droneResult.Value.TakeOff();
    }

    public Result Land(int droneId)
    {
        var droneResult = _repository.GetById(droneId);
        if (!droneResult.IsSuccess || droneResult.Value is null)
        {
            return Result.Failure(droneResult.Error ?? "Drone not found.", droneResult.ErrorCode ?? ResultCodes.NotFound);
        }

        if (!droneResult.Value.IsAirborne)
        {
            return Result.Failure("Drone is already grounded.", ResultCodes.Validation);
        }

        droneResult.Value.Land();
        return Result.Success();
    }

    public Result SetWaypoint(int droneId, double latitude, double longitude)
    {
        var droneResult = _repository.GetById(droneId);
        if (!droneResult.IsSuccess || droneResult.Value is null)
        {
            return Result.Failure(droneResult.Error ?? "Drone not found.", droneResult.ErrorCode ?? ResultCodes.NotFound);
        }

        return droneResult.Value switch
        {
            DeliveryDrone delivery => delivery.SetWaypoint(latitude, longitude),
            SurveyDrone survey => survey.SetWaypoint(latitude, longitude),
            _ => Result.Failure("Drone type does not support waypoints.", ResultCodes.Validation)
        };
    }

    public Result UpdateCargoLoad(int droneId, double kilograms)
    {
        var droneResult = _repository.GetById(droneId);
        if (!droneResult.IsSuccess || droneResult.Value is null)
        {
            return Result.Failure(droneResult.Error ?? "Drone not found.", droneResult.ErrorCode ?? ResultCodes.NotFound);
        }

        if (droneResult.Value is DeliveryDrone delivery)
        {
            return delivery.UpdateLoad(kilograms);
        }

        return Result.Failure("Drone type does not support cargo load updates.", ResultCodes.Validation);
    }

    public Result UnloadCargo(int droneId)
    {
        var droneResult = _repository.GetById(droneId);
        if (!droneResult.IsSuccess || droneResult.Value is null)
        {
            return Result.Failure(droneResult.Error ?? "Drone not found.", droneResult.ErrorCode ?? ResultCodes.NotFound);
        }

        if (droneResult.Value is DeliveryDrone delivery)
        {
            return delivery.UnloadAll();
        }

        return Result.Failure("Drone type does not support cargo unloading.", ResultCodes.Validation);
    }

    public Result CapturePhoto(int droneId)
    {
        var droneResult = _repository.GetById(droneId);
        if (!droneResult.IsSuccess || droneResult.Value is null)
        {
            return Result.Failure(droneResult.Error ?? "Drone not found.", droneResult.ErrorCode ?? ResultCodes.NotFound);
        }

        if (droneResult.Value is SurveyDrone survey)
        {
            return survey.TakePhoto();
        }

        return Result.Failure("Drone type does not support photo capture.", ResultCodes.Validation);
    }

    public DroneFleetSummary GetSummary()
    {
        return _repository.List().ToFleetSummary();
    }

    public async Task<Result> ExportToJsonAsync(string destinationPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            return Result.Failure("Destination path cannot be empty.", ResultCodes.Validation);
        }

        var snapshots = _repository.List()
            .OrderBy(drone => drone.Id)
            .Select(drone => drone.ToSnapshot())
            .ToArray();

        var path = ResolveDestinationPath(destinationPath, GetCandidateRoots());
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await JsonSerializer.SerializeAsync(stream, snapshots, _jsonOptions, cancellationToken);
        await stream.FlushAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ExportToCsvAsync(string destinationPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            return Result.Failure("Destination path cannot be empty.", ResultCodes.Validation);
        }

        var path = ResolveDestinationPath(destinationPath, GetCandidateRoots());
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var builder = new StringBuilder();
    builder.AppendLine("Id,Name,Kind,BatteryPercent,IsAirborne,LoadKg,WaypointLat,WaypointLon,PhotoCount");

        foreach (var snapshot in _repository.List().OrderBy(drone => drone.Id).Select(drone => drone.ToSnapshot()))
        {
            cancellationToken.ThrowIfCancellationRequested();
            builder.Append(snapshot.Id).Append(',')
                .Append(QuoteIfRequired(snapshot.Name)).Append(',')
                .Append(snapshot.Kind).Append(',')
                .Append(snapshot.BatteryPercent.ToString(Culture)).Append(',')
                .Append(snapshot.IsAirborne ? "true" : "false").Append(',')
                .Append(snapshot.LoadKg?.ToString(Culture) ?? string.Empty).Append(',')
                .Append(snapshot.WaypointLat?.ToString(Culture) ?? string.Empty).Append(',')
                .Append(snapshot.WaypointLon?.ToString(Culture) ?? string.Empty).Append(',')
                .Append(snapshot.PhotoCount?.ToString(Culture) ?? string.Empty)
                .AppendLine();
        }

        await File.WriteAllTextAsync(path, builder.ToString(), cancellationToken);
        return Result.Success();
    }

    private static string QuoteIfRequired(string value)
    {
        if (value.Contains(',') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static IReadOnlyList<string> GetCandidateRoots()
    {
        var roots = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        static IEnumerable<string> EnumerateAncestors(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                yield break;
            }

            var current = new DirectoryInfo(root);
            while (current is not null && current.Exists)
            {
                yield return current.FullName;
                current = current.Parent;
            }
        }

        void Add(string? candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return;
            }

            var full = Path.GetFullPath(candidate);
            if (seen.Add(full))
            {
                roots.Add(full);
            }
        }

        Add(FindSolutionRoot());
        Add(Directory.GetCurrentDirectory());
        Add(AppContext.BaseDirectory);

        foreach (var ancestor in EnumerateAncestors(Directory.GetCurrentDirectory()))
        {
            Add(ancestor);
        }

        foreach (var ancestor in EnumerateAncestors(AppContext.BaseDirectory))
        {
            Add(ancestor);
        }

        if (roots.Count == 0)
        {
            Add(Environment.CurrentDirectory);
        }

        return roots;
    }

    private static string ResolveExistingPath(string rawPath, IReadOnlyList<string> candidateRoots)
    {
        var trimmed = (rawPath ?? string.Empty).Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return trimmed;
        }

        if (Path.IsPathRooted(trimmed))
        {
            return Path.GetFullPath(trimmed);
        }

        foreach (var root in candidateRoots)
        {
            var candidate = Path.GetFullPath(Path.Combine(root, trimmed));
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var fallbackRoot = candidateRoots.Count > 0 ? candidateRoots[0] : Directory.GetCurrentDirectory();
        return Path.GetFullPath(Path.Combine(fallbackRoot, trimmed));
    }

    private static string ResolveDestinationPath(string rawPath, IReadOnlyList<string> candidateRoots)
    {
        var trimmed = (rawPath ?? string.Empty).Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return trimmed;
        }

        if (Path.IsPathRooted(trimmed))
        {
            return Path.GetFullPath(trimmed);
        }

        foreach (var root in candidateRoots)
        {
            var candidate = Path.GetFullPath(Path.Combine(root, trimmed));
            var directory = Path.GetDirectoryName(candidate);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            {
                return candidate;
            }
        }

        var fallbackRoot = candidateRoots.Count > 0 ? candidateRoots[0] : Directory.GetCurrentDirectory();
        return Path.GetFullPath(Path.Combine(fallbackRoot, trimmed));
    }

    private static string? FindSolutionRoot()
    {
        static string? FindFrom(string? start)
        {
            if (string.IsNullOrWhiteSpace(start))
            {
                return null;
            }

            var current = new DirectoryInfo(start);
            while (current is not null && current.Exists)
            {
                if (current.EnumerateFiles("*.sln", SearchOption.TopDirectoryOnly).Any())
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            return null;
        }

        return FindFrom(AppContext.BaseDirectory) ?? FindFrom(Directory.GetCurrentDirectory());
    }
}
