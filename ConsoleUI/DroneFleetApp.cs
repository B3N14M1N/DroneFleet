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
    private readonly DroneCreationRegistry _registry;
    private readonly CapabilityRegistry _capabilityRegistry;
    private readonly Dictionary<string, Type> _droneCategories;
    private readonly MenuHandlers _menuHandlers;

    public DroneFleetApp()
    {
        _droneFactory = new DroneFactory();
        _droneManager = new DroneManager();

        _registry = new DroneCreationRegistry(
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

        _menuHandlers = new MenuHandlers(
            _droneManager,
            _droneFactory,
            _registry,
            _capabilityRegistry,
            _droneCategories);
    }

    /// <summary>
    /// Runs the main application loop.
    /// </summary>
    public void Run()
    {
        while (true)
        {
            DisplayMenu();
            var choice = Console.ReadLine();

            if (!ProcessMenuChoice(choice))
            {
                return; // Exit application
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Displays the main menu.
    /// </summary>
    private static void DisplayMenu()
    {
        Console.Clear();
        Console.Write(
            """
            Drone Fleet Management System Menu:
            1. List drones
            2. Add drone
            3. Pre-flight check (all)
            4. Take off / Land
            5. Set waypoint
            6. Capability actions
            7. Charge battery
            8. Exit
            Select an option: 
            """);
    }

    /// <summary>
    /// Processes the user's menu choice.
    /// </summary>
    /// <param name="choice">The menu choice entered by the user.</param>
    /// <returns>True to continue the application loop; false to exit.</returns>
    private bool ProcessMenuChoice(string? choice)
    {
        switch (choice)
        {
            case "1":
                _menuHandlers.HandleListDrones();
                break;

            case "2":
                _menuHandlers.HandleAddDrone();
                break;

            case "3":
                _menuHandlers.HandlePreFlightCheck();
                break;

            case "4":
                _menuHandlers.HandleFlightControl();
                break;

            case "5":
                _menuHandlers.HandleSetWaypoint();
                break;

            case "6":
                _menuHandlers.HandleCapabilityActions();
                break;

            case "7":
                _menuHandlers.HandleChargeBattery();
                break;

            case "8":
                return false;

            default:
                Console.WriteLine("\nInvalid option. Please try again.");
                break;
        }

        return true;
    }
}
