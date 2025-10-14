using DroneFleet.ConsoleUI;
using DroneFleet.Models;
using DroneFleet.Models.Interfaces;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.Services.Capabilities;

/// <summary>
/// Handles photo capture actions for drones that support <see cref="IPhotoCapture"/>.
/// </summary>
internal sealed class PhotoCaptureCapabilityHandler : ICapabilityActionHandler
{
    public string Key => "photo_capture";

    public string DisplayName => "Photo Capture";

    public bool Supports(Drone drone) => drone is IPhotoCapture;

    public void Execute(Drone drone)
    {
        if (drone is not IPhotoCapture photoCapture)
        {
            InputHelpers.PrintError("Photo capture capability is not available for this drone.");
            return;
        }

        photoCapture.TakePhoto();
        InputHelpers.PrintSuccess($"Photo taken! Total photos: {photoCapture.PhotoCount}. Battery: {drone.BatteryPercent}%");
    }
}
