using DroneFleet.Domain.Common;

namespace DroneFleet.App.ConsoleApp.Updates;

/// <summary>
/// Represents the outcome of executing a drone update handler.
/// </summary>
internal sealed record DroneUpdateResponse(Result Result, string? SuccessMessage);
