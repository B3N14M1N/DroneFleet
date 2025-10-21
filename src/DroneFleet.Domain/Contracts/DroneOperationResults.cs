using DroneFleet.Domain.Models;

namespace DroneFleet.Domain.Contracts;

public sealed record DroneSelfTestResult(Drone Drone, bool Passed, string? ErrorMessage);

public sealed record DroneChargeResult(Drone Drone, bool Success, double? BatteryLevel, string? ErrorMessage);
