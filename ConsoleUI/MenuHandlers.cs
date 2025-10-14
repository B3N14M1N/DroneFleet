using DroneFleet.Models.Interfaces;
using DroneFleet.Services;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.ConsoleUI;

/// <summary>
/// Contains all menu action handlers for the Drone Fleet Management System.
/// </summary>
internal class MenuHandlers(
    IDroneManager droneManager,
    IDroneFactory droneFactory,
    DroneCreationRegistry registry,
    Dictionary<string, Type> droneCategories)
{
    private readonly IDroneManager _droneManager = droneManager;
    private readonly IDroneFactory _droneFactory = droneFactory;
    private readonly DroneCreationRegistry _registry = registry;
    private readonly Dictionary<string, Type> _droneCategories = droneCategories;

    /// <summary>
    /// Handles the list drones menu option.
    /// </summary>
    public void HandleListDrones()
    {
        Console.WriteLine();
        Console.WriteLine("Listing all drones:");
        try
        {
            _droneManager.ListDronesByCategory(_droneCategories);
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error listing drones: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the add drone menu option.
    /// </summary>
    public void HandleAddDrone()
    {
        Console.WriteLine();
        Console.WriteLine("Available types:");
        
        // Create a list to map numbers to creators
        var creatorsList = _registry.All.ToList();
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
                return; // User cancelled
            }

            // Try to parse as number first
            if (int.TryParse(input, out int number) && number >= 1 && number <= creatorsList.Count)
            {
                selectedCreator = creatorsList[number - 1];
            }
            // Try to get by key (name)
            else if (_registry.TryGet(input, out var creator))
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
    }

    /// <summary>
    /// Handles the pre-flight check menu option.
    /// </summary>
    public void HandlePreFlightCheck()
    {
        Console.WriteLine();
        Console.WriteLine("Running pre-flight checks on all drones:");
        try
        {
            _droneManager.PreFlightCheckAll();
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error during pre-flight check: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the flight control menu option (take off/land).
    /// </summary>
    public void HandleFlightControl()
    {
        Console.WriteLine();
        var drone = InputHelpers.PromptForDrone(_droneManager);
        if (drone == null) return;

        Console.WriteLine($"Current status: {(drone.IsAirborne ? "Airborne" : "Landed")}");
        
        var action = InputHelpers.PromptForOption("Action (1=TakeOff, 2=Land)", 2);
        if (action == null) return;

        try
        {
            switch (action.Value)
            {
                case 1:
                    drone.TakeOff();
                    InputHelpers.PrintSuccess($"{drone.Name} took off. Battery: {drone.BatteryPercent}%");
                    break;
                case 2:
                    drone.Land();
                    InputHelpers.PrintSuccess($"{drone.Name} landed. Battery: {drone.BatteryPercent}%");
                    break;
            }
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the set waypoint menu option.
    /// </summary>
    public void HandleSetWaypoint()
    {
        Console.WriteLine();
        var drone = InputHelpers.PromptForDrone(_droneManager);
        if (drone == null) return;

        if (drone is not INavigable navigableDrone)
        {
            InputHelpers.PrintError($"{drone.Name} does not support navigation.");
            return;
        }

        var coordinates = InputHelpers.PromptForCoordinates();
        if (coordinates == null) return;

        try
        {
            var (lat, lon) = coordinates.Value;
            navigableDrone.SetWaypoint(lat, lon);
            InputHelpers.PrintSuccess($"Waypoint set to ({lat}, {lon}) for {drone.Name}. Battery: {drone.BatteryPercent}%");
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error setting waypoint: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the capability actions menu option.
    /// </summary>
    public void HandleCapabilityActions()
    {
        Console.WriteLine();
        var drone = InputHelpers.PromptForDrone(_droneManager);
        if (drone == null) return;

        (int, int)? capabilitiesRange = null;
        Console.WriteLine($"\nCapabilities for {drone.Name}:");

        if (drone is IPhotoCapture)
        {
            Console.WriteLine("1. Photo Capture");
            capabilitiesRange = (1, 1);
        }

        if (drone is ICargoCarrier)
        {
            Console.WriteLine("1. Load Cargo");
            Console.WriteLine("2. Unload All Cargo");
            capabilitiesRange = (1, 2);
        }
        
        if (capabilitiesRange == null)
        {
            InputHelpers.PrintError("This drone has no special capabilities.");
            return;
        }

        var action = InputHelpers.PromptForOption("\nSelect capability action", capabilitiesRange.Value.Item2, capabilitiesRange.Value.Item1);
        if (action == null) return;

        try
        {
            switch (action.Value)
            {
                case 1 when drone is IPhotoCapture photoCapture:
                    CapabilityHandlers.HandlePhotoCapture(photoCapture, drone);
                    break;
                case 1 when drone is ICargoCarrier cargoCarrier:
                    CapabilityHandlers.HandleCargoLoad(cargoCarrier);
                    break;
                case 2 when drone is ICargoCarrier cargoCarrier:
                    CapabilityHandlers.HandleCargoUnload(cargoCarrier);
                    break;
                default:
                    InputHelpers.PrintError("Invalid action or capability not supported.");
                    break;
            }
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the charge battery menu option.
    /// </summary>
    public void HandleChargeBattery()
    {
        Console.WriteLine();
        var chargeInput = InputHelpers.PromptForString("Enter drone ID (or 'all' for all drones, or press Enter to cancel): ");
        if (string.IsNullOrEmpty(chargeInput)) return;

        var chargeAmount = InputHelpers.PromptForDouble("Enter % charge amount (or press Enter to cancel): ", min: 0, max: 100);
        if (chargeAmount == null) return;

        try
        {
            if (chargeInput.Equals("all", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine();
                _droneManager.ChargeAllDrones(chargeAmount.Value);
            }
            else if (int.TryParse(chargeInput, out int chargeDroneId))
            {
                var drone = _droneManager.GetDroneById(chargeDroneId);
                if (drone == null)
                {
                    InputHelpers.PrintError("Drone not found.");
                    return;
                }

                drone.ChargeBattery(chargeAmount.Value);
                InputHelpers.PrintSuccess($"Drone ID: {drone.Id}, New Battery Level: {drone.BatteryPercent}%");
            }
            else
            {
                InputHelpers.PrintError("Invalid input.");
            }
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error charging battery: {ex.Message}");
        }
    }
}
