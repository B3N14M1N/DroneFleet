using DroneFleet.Domain.Models;
using DroneFleet.Domain.Models.Interfaces;
using DroneFleet.Infrastructure.Services.Interfaces;

namespace DroneFleet.App.ConsoleUI.Capabilities;

/// <summary>
/// Handles unloading cargo for drones that implement <see cref="ICargoCarrier"/>.
/// </summary>
public sealed class CargoUnloadCapabilityHandler : ICapabilityActionHandler
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
