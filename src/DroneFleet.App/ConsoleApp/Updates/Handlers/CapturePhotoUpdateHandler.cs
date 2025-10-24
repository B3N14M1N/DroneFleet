using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Updates.Handlers;

/// <summary>
/// Handles photo capture operations for survey drones.
/// </summary>
internal sealed class CapturePhotoUpdateHandler : IDroneUpdateHandler
{
    public string Keyword => "capture";

    public bool Supports(Drone drone) => drone is SurveyDrone;

    public DroneUpdateResponse Execute(CommandContext context, Drone drone, IReadOnlyList<string> arguments)
    {
        if (arguments.Count > 0)
        {
            return new DroneUpdateResponse(Result.Failure("Photo capture does not accept parameters.", ResultCodes.Validation), null);
        }

        var result = context.FleetService.CapturePhoto(drone.Id);
        if (!result.IsSuccess)
        {
            return new DroneUpdateResponse(result, null);
        }

        var updated = context.FleetService.GetDrone(drone.Id);
        if (updated.IsSuccess && updated.Value is SurveyDrone survey)
        {
            return new DroneUpdateResponse(result, $"Drone {survey.Id} captured a photo. Total: {survey.PhotoCount}.");
        }

        return new DroneUpdateResponse(result, $"Drone {drone.Id} captured a photo.");
    }
}
