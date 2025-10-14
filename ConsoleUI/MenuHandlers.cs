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
    DroneCreationRegistry creationRegistry,
    CapabilityRegistry capabilityRegistry,
    Dictionary<string, Type> droneCategories)
{
    private readonly IDroneManager _droneManager = droneManager;
    private readonly IDroneFactory _droneFactory = droneFactory;
    private readonly DroneCreationRegistry _creationRegistry = creationRegistry;
    private readonly CapabilityRegistry _capabilityRegistry = capabilityRegistry;
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
            var drones = _droneManager.Drones.ToList();

            if (drones.Count == 0)
            {
                Console.WriteLine("No drones available.");
                return;
            }

            foreach (var (categoryName, droneType) in _droneCategories)
            {
                var dronesInCategory = drones
                    .Where(d => d.GetType() == droneType)
                    .ToList();

                if (dronesInCategory.Count == 0)
                {
                    continue;
                }

                Console.WriteLine($"=== {categoryName} ===");
                foreach (var drone in dronesInCategory)
                {
                    Console.WriteLine(drone);
                }
                Console.WriteLine();
            }

            var categorizedTypes = _droneCategories.Values.ToHashSet();
            var uncategorized = drones
                .Where(d => !categorizedTypes.Contains(d.GetType()))
                .ToList();

            if (uncategorized.Count > 0)
            {
                Console.WriteLine("=== Other Drones ===");
                foreach (var drone in uncategorized)
                {
                    Console.WriteLine(drone);
                }
                Console.WriteLine();
            }
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
                return;
            }

            // Try to parse as number first
            if (int.TryParse(input, out int number) && number >= 1 && number <= creatorsList.Count)
            {
                selectedCreator = creatorsList[number - 1];
            }
            // Try to get by key (name)
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
            var results = _droneManager.PreFlightCheckAll().ToList();

            if (results.Count == 0)
            {
                Console.WriteLine("No drones available.");
                return;
            }

            foreach (var result in results)
            {
                if (result.Passed)
                {
                    InputHelpers.PrintSuccess($"Drone ID: {result.Drone.Id}, Self-test passed.");
                }
                else
                {
                    var message = result.ErrorMessage ?? "Self-test failed.";
                    InputHelpers.PrintError($"Drone ID: {result.Drone.Id}, Self-test failed: {message}");
                }
            }
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

        var handlers = _capabilityRegistry
            .GetHandlersFor(drone)
            .ToList();

        if (handlers.Count == 0)
        {
            InputHelpers.PrintError("This drone has no special capabilities.");
            return;
        }

        Console.WriteLine($"\nCapabilities for {drone.Name}:");
        for (int index = 0; index < handlers.Count; index++)
        {
            Console.WriteLine($"{index + 1}. {handlers[index].DisplayName}");
        }

        var selection = InputHelpers.PromptForOption("\nSelect capability action", handlers.Count);
        if (selection == null)
        {
            return;
        }

        var handler = handlers[selection.Value - 1];

        try
        {
            handler.Execute(drone);
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
                var results = _droneManager.ChargeAllDrones(chargeAmount.Value).ToList();

                if (results.Count == 0)
                {
                    Console.WriteLine("No drones available.");
                    return;
                }

                foreach (var result in results)
                {
                    if (result.Success)
                    {
                        InputHelpers.PrintSuccess($"Drone ID: {result.Drone.Id}, New Battery Level: {result.BatteryLevel}%");
                    }
                    else
                    {
                        var message = result.ErrorMessage ?? "Charging failed.";
                        InputHelpers.PrintError($"Drone ID: {result.Drone.Id}, Charging failed: {message}");
                    }
                }
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
