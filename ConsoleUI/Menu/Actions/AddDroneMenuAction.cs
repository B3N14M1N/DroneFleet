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

    public string Description => "Create and add a new drone";

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

        IDroneCreator? selectedCreator = null;

        while (selectedCreator == null)
        {
            Console.WriteLine("\nEnter drone type (number or name) or press Enter to cancel: ");
            Console.Write(">>> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return MenuActionOutcome.Continue;
            }

            if (int.TryParse(input, out int number) && number >= 1 && number <= creatorsList.Count)
            {
                selectedCreator = creatorsList[number - 1];
            }
            else if (_creationRegistry.TryGet(input, out var creator))
            {
                selectedCreator = creator;
            }
            else
            {
                InputHelpers.PrintError("Unknown type. Please try again.");
            }
        }

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
