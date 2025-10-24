using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;
using System.Globalization;

namespace DroneFleet.Infrastructure.FileIO;

/// <summary>
/// Parses CSV entries into <see cref="DroneSnapshot"/> instances.
/// </summary>
internal sealed class DroneCsvParser
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public Result<DroneSnapshot> Parse(IReadOnlyList<string> columns)
    {
        if (columns.Count < 9)
        {
            return Result<DroneSnapshot>.Failure("Row does not contain the expected number of columns.", ResultCodes.Validation);
        }

        if (!int.TryParse(columns[0], NumberStyles.Integer, Culture, out var id))
        {
            return Result<DroneSnapshot>.Failure("Invalid Id value.", ResultCodes.Validation);
        }

        var name = columns[1].Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<DroneSnapshot>.Failure("Name cannot be empty.", ResultCodes.Validation);
        }

        if (!TryParseKind(columns[2], out var kind))
        {
            return Result<DroneSnapshot>.Failure("Unknown drone type.", ResultCodes.Validation);
        }

        if (!double.TryParse(columns[3], NumberStyles.Float, Culture, out var battery))
        {
            return Result<DroneSnapshot>.Failure("Invalid battery percent.", ResultCodes.Validation);
        }

        if (!TryParseBoolean(columns[4], out var isAirborne))
        {
            return Result<DroneSnapshot>.Failure("Invalid airborne flag.", ResultCodes.Validation);
        }

        var load = TryParseNullableDouble(columns[5]);
        var waypointLat = TryParseNullableDouble(columns[6]);
        var waypointLon = TryParseNullableDouble(columns[7]);
        var photoCount = TryParseNullableInt(columns[8]);

        var snapshot = new DroneSnapshot(
            id,
            name,
            kind,
            battery,
            isAirborne,
            load,
            waypointLat,
            waypointLon,
            photoCount);

        return Result<DroneSnapshot>.Success(snapshot);
    }

    private static bool TryParseKind(string value, out DroneKind kind)
    {
        if (Enum.TryParse<DroneKind>(value, true, out kind))
        {
            return true;
        }

        var trimmed = value.Trim().ToLowerInvariant();
        switch (trimmed)
        {
            case "delivery":
                kind = DroneKind.Delivery;
                return true;
            case "survey":
                kind = DroneKind.Survey;
                return true;
            case "racing":
                kind = DroneKind.Racing;
                return true;
            default:
                kind = default;
                return false;
        }
    }

    private static bool TryParseBoolean(string value, out bool result)
    {
        if (bool.TryParse(value, out result))
        {
            return true;
        }

        var trimmed = value.Trim().ToLowerInvariant();
        return trimmed switch
        {
            "1" or "yes" or "y" => result = true,
            "0" or "no" or "n" => result = false,
            _ => false
        };
    }

    private static double? TryParseNullableDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return double.TryParse(value, NumberStyles.Float, Culture, out var result) ? result : null;
    }

    private static int? TryParseNullableInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value, NumberStyles.Integer, Culture, out var result) ? result : null;
    }
}
