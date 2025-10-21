using DroneFleet.App.ConsoleUI;
using DroneFleet.Domain.Models;
using DroneFleet.Infrastructure.Services.Interfaces;

namespace DroneFleet.App.ConsoleUI.Creators;

internal sealed class RacingDroneCreator : IDroneCreator
{
    public string Key => "racing";
    public string DisplayName => "Racing Drone";

    /// <inheritdoc/>
    public Drone CreateInteractive(IDroneFactory factory)
    {
        var name = InputHelpers.PromptForString("Enter name", "Unnamed Racing Drone");
        return factory.Create<RacingDrone>(name);
    }
}
