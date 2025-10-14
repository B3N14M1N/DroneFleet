using DroneFleet.Models;
using DroneFleet.Models.Interfaces;

namespace DroneFleet.ConsoleUI;

/// <summary>
/// Provides handlers for drone capability-specific actions.
/// </summary>
internal static class CapabilityHandlers
{

    /// <summary>
    /// Handles the photo capture capability action.
    /// </summary>
    /// <param name="photoCapture">The photo capture capability interface.</param>
    /// <param name="drone">The drone performing the action.</param>
    public static void HandlePhotoCapture(IPhotoCapture photoCapture, Drone drone)
    {
        photoCapture.TakePhoto();
        InputHelpers.PrintSuccess($"Photo taken! Total photos: {photoCapture.PhotoCount}. Battery: {drone.BatteryPercent}%");
    }

    /// <summary>
    /// Handles the cargo loading capability action.
    /// </summary>
    /// <param name="cargoCarrier">The cargo carrier capability interface.</param>
    public static void HandleCargoLoad(ICargoCarrier cargoCarrier)
    {
        var capacityLeft = cargoCarrier.CapacityKg - cargoCarrier.CurrentLoadKg;
        var kg = InputHelpers.PromptForDouble("Enter weight to load (kg) (or press Enter to cancel): ", min: 0, max: capacityLeft);

        if (!kg.HasValue)
            return;

        if (cargoCarrier.Load(kg.Value))
        {
            InputHelpers.PrintSuccess($"Loaded {kg:F2} kg. Current load: {cargoCarrier.CurrentLoadKg:F2}/{cargoCarrier.CapacityKg:F2} kg");
        }
        else
        {
            InputHelpers.PrintError("Load failed. Check capacity or ensure drone is landed.");
        }
    }

    /// <summary>
    /// Handles the cargo unloading capability action.
    /// </summary>
    /// <param name="cargoCarrier">The cargo carrier capability interface.</param>
    public static void HandleCargoUnload(ICargoCarrier cargoCarrier)
    {
        cargoCarrier.UnloadAll();
        InputHelpers.PrintSuccess("All cargo unloaded.");
    }
}
