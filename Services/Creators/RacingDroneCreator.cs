using DroneFleet.Models;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.Services.Creators;

internal sealed class RacingDroneCreator : IDroneCreator
{
    public string Key => "racing";
    public string DisplayName => "Racing Drone";

    /// <inheritdoc/>
    public Drone CreateInteractive(IDroneFactory factory)
    {
        Console.Write("Enter name: ");
        var name = Console.ReadLine() ?? "Unnamed Racing Drone";
        return factory.Create<RacingDrone>(name);
    }
}
