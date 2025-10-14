using DroneFleet.ConsoleUI;
using DroneFleet.Models;
using DroneFleet.Models.Interfaces;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.Services.Capabilities;

/// <summary>
/// Handles unloading cargo for drones that implement <see cref="ICargoCarrier"/>.
/// </summary>
internal sealed class CargoUnloadCapabilityHandler : ICapabilityActionHandler
{
    public string Key => "cargo_unload";

    public string DisplayName => "Unload All Cargo";

    public bool Supports(Drone drone) => drone is ICargoCarrier;

    public void Execute(Drone drone)
    {
        if (drone is not ICargoCarrier cargoCarrier)
        {
            InputHelpers.PrintError("Cargo unloading is not available for this drone.");
            return;
        }

        cargoCarrier.UnloadAll();
        InputHelpers.PrintSuccess("All cargo unloaded.");
    }
}
