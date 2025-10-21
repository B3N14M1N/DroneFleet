using DroneFleet.Contracts;
using DroneFleet.Models;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.ConsoleUI.Creators;

internal sealed class DeliveryDroneCreator : IDroneCreator
{
    public string Key => "delivery";
    public string DisplayName => "Delivery Drone";

    /// <inheritdoc/>
    public Drone CreateInteractive(IDroneFactory factory)
    {
        var name = InputHelpers.PromptForString("Enter name", "Unnamed Delivery Drone");
        var capacity = InputHelpers.PromptForDouble("Capacity kg (>= 0)", defaultValue: 5.0, min: 0);

        var opts = new DroneCreationOptions { CapacityKg = capacity };
        return factory.Create<DeliveryDrone>(name, opts);
    }
}
