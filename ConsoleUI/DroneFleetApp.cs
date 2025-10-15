using DroneFleet.ConsoleUI.Menu;
using DroneFleet.ConsoleUI.Menu.Actions;
using DroneFleet.Models;
using DroneFleet.Services;
using DroneFleet.Services.Capabilities;
using DroneFleet.Services.Creators;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.ConsoleUI;

/// <summary>
/// Main application class for the Drone Fleet Management System.
/// </summary>
internal class DroneFleetApp
{
    private readonly IDroneFactory _droneFactory;
    private readonly IDroneManager _droneManager;
    private readonly IDroneRepository _droneRepository;
    private readonly DroneCreationRegistry _creationRegistry;
    private readonly CapabilityRegistry _capabilityRegistry;
    private readonly Dictionary<string, Type> _droneCategories;
    private readonly MenuActionRegistry _menuActionRegistry;

    public DroneFleetApp()
    {
        _droneFactory = new DroneFactory();
        _droneRepository = new DroneRepository();
        _droneManager = new DroneManager(_droneRepository);

        _creationRegistry = new DroneCreationRegistry(
        [
            new DeliveryDroneCreator(),
            new SurveyDroneCreator(),
            new RacingDroneCreator(),
        ]);

        _capabilityRegistry = new CapabilityRegistry(
        [
            new PhotoCaptureCapabilityHandler(),
            new CargoLoadCapabilityHandler(),
            new CargoUnloadCapabilityHandler(),
        ]);

        _droneCategories = new Dictionary<string, Type>
        {
            { "Delivery Drones", typeof(DeliveryDrone) },
            { "Survey Drones", typeof(SurveyDrone) },
            { "Racing Drones", typeof(RacingDrone) }
        };

        _menuActionRegistry = new MenuActionRegistry();

        _menuActionRegistry.Register(new ListDronesMenuAction(_droneManager, _droneCategories));
        _menuActionRegistry.Register(new AddDroneMenuAction(_droneManager, _droneFactory, _creationRegistry));
        _menuActionRegistry.Register(new PreFlightCheckMenuAction(_droneManager));
        _menuActionRegistry.Register(new FlightControlMenuAction(_droneManager));
        _menuActionRegistry.Register(new SetWaypointMenuAction(_droneManager));
        _menuActionRegistry.Register(new CapabilityActionsMenuAction(_droneManager, _capabilityRegistry));
        _menuActionRegistry.Register(new ChargeBatteryMenuAction(_droneManager));
        _menuActionRegistry.Register(new ExitMenuAction());
    }

    /// <summary>
    /// Runs the main application loop.
    /// </summary>
    public void Run()
    {
        while (true)
        {
            var actions = _menuActionRegistry.Actions;
            DisplayMenu(actions);

            var selection = InputHelpers.PromptForOption("Select an option", actions.Count);
            if (selection == null)
            {
                continue;
            }

            var action = actions[selection.Value - 1];
            var outcome = action.Execute();
            if (outcome == MenuActionOutcome.Exit)
            {
                return;
            }

            PauseForUser();
        }
    }

    /// <summary>
    /// Displays the main menu.
    /// </summary>
    private static void DisplayMenu(IReadOnlyList<IMenuAction> actions)
    {
        Console.Clear();
        Console.WriteLine("Drone Fleet Management System Menu:");

        for (int index = 0; index < actions.Count; index++)
        {
            var menuAction = actions[index];
            var optionNumber = index + 1;
            Console.WriteLine($"{optionNumber}. {menuAction.Label}");
            if (!string.IsNullOrWhiteSpace(menuAction.Description))
            {
                Console.WriteLine($"   {menuAction.Description}");
            }
        }
    }

    private static void PauseForUser()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
