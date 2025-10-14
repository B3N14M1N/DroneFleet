using DroneFleet.Models;

namespace DroneFleet.Services.Models;

internal sealed record DroneSelfTestResult(Drone Drone, bool Passed, string? ErrorMessage);

internal sealed record DroneChargeResult(Drone Drone, bool Success, double? BatteryLevel, string? ErrorMessage);
