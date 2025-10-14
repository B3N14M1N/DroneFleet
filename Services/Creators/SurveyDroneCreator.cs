using DroneFleet.Models;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.Services.Creators;

internal sealed class SurveyDroneCreator : IDroneCreator
{
    public string Key => "survey";
    public string DisplayName => "Survey Drone";

    /// <inheritdoc/>
    public Drone CreateInteractive(IDroneFactory factory)
    {
        Console.Write("Enter name: ");
        var name = Console.ReadLine() ?? "Unnamed Survey Drone";
        return factory.Create<SurveyDrone>(name);
    }
}
