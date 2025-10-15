using DroneFleet.Services;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.ConsoleUI.Menu.Actions;

internal sealed class AddDroneMenuAction(
    IDroneManager droneManager,
    IDroneFactory droneFactory,
    DroneCreationRegistry creationRegistry) : IMenuAction
{
    private readonly IDroneManager _droneManager = droneManager;
    private readonly IDroneFactory _droneFactory = droneFactory;
    private readonly DroneCreationRegistry _creationRegistry = creationRegistry;

    public string Label => "Add drone";

    /// <inheritdoc/>
    public MenuActionOutcome Execute()
    {
        ArgumentNullException.ThrowIfNull(_droneManager);
        ArgumentNullException.ThrowIfNull(_droneFactory);
        ArgumentNullException.ThrowIfNull(_creationRegistry);

        Console.WriteLine();
        Console.WriteLine("Available types:");

        var creatorsList = _creationRegistry.All.ToList();
        for (int i = 0; i < creatorsList.Count; i++)
        {
            var creator = creatorsList[i];
            Console.WriteLine($"{i + 1}. {creator.Key} ({creator.DisplayName})");
        }

        var creatorKeys = creatorsList.Select(c => c.Key).ToArray();
        var selectedIndex = InputHelpers.PromptForOption(
            "Enter drone type (number or name) (or press Enter to cancel): ",
            creatorsList.Count,
            validStrings:creatorKeys);

        if (selectedIndex == null)
        {
            return MenuActionOutcome.Continue;
        }

        var selectedCreator = creatorsList[selectedIndex.Value - 1];

        try
        {
            var newDrone = selectedCreator.CreateInteractive(_droneFactory);
            _droneManager.AddDrone(newDrone);
            InputHelpers.PrintSuccess($"Added {newDrone.GetType().Name} '{newDrone.Name}' with ID {newDrone.Id}.");
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error adding drone: {ex.Message}");
        }

        return MenuActionOutcome.Continue;
    }
}
