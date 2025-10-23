namespace DroneFleet.Domain.Operations;

/// <summary>
/// Represents the outcome of importing drone data files.
/// </summary>
public sealed record FleetImportResult(
    int FilesProcessed,
    int TotalRows,
    int Imported,
    int Duplicates,
    int Malformed,
    IReadOnlyList<ImportIssue> Issues)
{
    /// <summary>
    /// Indicates whether at least one record was imported.
    /// </summary>
    public bool HasImports => Imported > 0;
}

/// <summary>
/// Represents an import parsing or validation issue.
/// </summary>
public sealed record ImportIssue(string Source, int LineNumber, string Message);
