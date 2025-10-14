using DroneFleet.Models;
using DroneFleet.Services.Interfaces;
using DroneFleet.ConsoleUI;

namespace DroneFleet.Services.Creators;

internal sealed class SurveyDroneCreator : IDroneCreator
{
    public string Key => "survey";
    public string DisplayName => "Survey Drone";

    /// <inheritdoc/>
    public Drone CreateInteractive(IDroneFactory factory)
    {
        var name = InputHelpers.PromptForString("Enter name", "Unnamed Survey Drone");
        return factory.Create<SurveyDrone>(name);
    }
}
