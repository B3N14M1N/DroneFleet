using DroneFleet.Domain.Models;
using DroneFleet.Domain.Models.Interfaces;
using DroneFleet.Infrastructure.Services.Interfaces;

namespace DroneFleet.App.ConsoleUI.Capabilities;

/// <summary>
/// Handles loading cargo for drones that implement <see cref="ICargoCarrier"/>.
/// </summary>
public sealed class CargoLoadCapabilityHandler : ICapabilityActionHandler
{
    public string Key => "cargo_load";

    public string DisplayName => "Load Cargo";

    public bool Supports(Drone drone) => drone is ICargoCarrier;

    public void Execute(Drone drone)
    {
        if (drone is not ICargoCarrier cargoCarrier)
        {
            InputHelpers.PrintError("Cargo loading is not available for this drone.");
            return;
        }

        var capacityLeft = Math.Max(0, cargoCarrier.CapacityKg - cargoCarrier.CurrentLoadKg);
        var kg = InputHelpers.PromptForDouble(
            "Enter weight to load (kg) (or press Enter to cancel): ",
            min: 0,
            max: capacityLeft);

        if (!kg.HasValue)
        {
            return;
        }

        if (cargoCarrier.Load(kg.Value))
        {
            InputHelpers.PrintSuccess($"Loaded {kg:F2} kg. Current load: {cargoCarrier.CurrentLoadKg:F2}/{cargoCarrier.CapacityKg:F2} kg");
        }
        else
        {
            InputHelpers.PrintError("Load failed. Check capacity or ensure drone is landed.");
        }
    }
}
