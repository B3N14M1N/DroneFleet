using DroneFleet.Services;
using DroneFleet.Services.Creators;
using DroneFleet.Services.Interfaces;
using DroneFleet.Models.Interfaces;
using DroneFleet.Models;

IDroneFactory droneFactory = new DroneFactory();
IDroneManager droneManager = new DroneManager();

var registry = new DroneCreationRegistry(
[
    new DeliveryDroneCreator(),
    new SurveyDroneCreator(),
    new RacingDroneCreator(),
]);

var droneCategories = new Dictionary<string, Type>
{
    { "Delivery Drones", typeof(DeliveryDrone) },
    { "Survey Drones", typeof(SurveyDrone) },
    { "Racing Drones", typeof(RacingDrone) }
};

while (true)
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

    switch (Console.ReadLine())
    {
        case "1":
            HandleListDrones();
            break;

        case "2":
            HandleAddDrone();
            break;

        case "3":
            HandlePreFlightCheck();
            break;

        case "4":
            HandleFlightControl();
            break;

        case "5":
            HandleSetWaypoint();
            break;

        case "6":
            HandleCapabilityActions();
            break;

        case "7":
            HandleChargeBattery();
            break;

        case "8":
            return;

        default:
            Console.WriteLine();
            Console.WriteLine("Invalid option. Please try again.");
            break;
    }

    Console.WriteLine();
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}


void HandleListDrones()
{
    Console.WriteLine();
    Console.WriteLine("Listing all drones:");
    try
    {
        droneManager.ListDronesByCategory(droneCategories);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error listing drones: {ex.Message}");
    }
}

void HandleAddDrone()
{
    Console.WriteLine();
    Console.WriteLine("Available types:");
    foreach (var c in registry.All)
        Console.WriteLine($"- {c.Key} ({c.DisplayName})");

    Console.Write("Enter drone type: ");
    var key = Console.ReadLine();

    if (!registry.TryGet(key ?? "", out var creator))
    {
        Console.WriteLine("Unknown type.");
        return;
    }

    try
    {
        var newDrone = creator.CreateInteractive(droneFactory);
        droneManager.AddDrone(newDrone);
        Console.WriteLine($"Added {newDrone.GetType().Name} '{newDrone.Name}' with ID {newDrone.Id}.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error adding drone: {ex.Message}");
    }
}

void HandlePreFlightCheck()
{
    Console.WriteLine();
    Console.WriteLine("Running pre-flight checks on all drones:");
    try
    {
        droneManager.PreFlightCheckAll();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during pre-flight check: {ex.Message}");
    }
}

void HandleFlightControl()
{
    Console.WriteLine();
    var drone = PromptForDrone();
    if (drone == null) return;

    Console.WriteLine($"Current status: {(drone.IsAirborne ? "Airborne" : "Landed")}");
    Console.Write("Action (1=TakeOff, 2=Land): ");
    var action = Console.ReadLine();

    try
    {
        switch (action)
        {
            case "1":
                drone.TakeOff();
                Console.WriteLine($"{drone.Name} took off. Battery: {drone.BatteryPercent}%");
                break;
            case "2":
                drone.Land();
                Console.WriteLine($"{drone.Name} landed. Battery: {drone.BatteryPercent}%");
                break;
            default:
                Console.WriteLine("Invalid action.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

void HandleSetWaypoint()
{
    Console.WriteLine();
    var drone = PromptForDrone();
    if (drone == null) return;

    if (drone is not INavigable navigableDrone)
    {
        Console.WriteLine($"{drone.Name} does not support navigation.");
        return;
    }

    var coordinates = PromptForCoordinates();
    if (coordinates == null) return;

    try
    {
        var (lat, lon) = coordinates.Value;
        navigableDrone.SetWaypoint(lat, lon);
        Console.WriteLine($"Waypoint set to ({lat}, {lon}) for {drone.Name}. Battery: {drone.BatteryPercent}%");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error setting waypoint: {ex.Message}");
    }
}

void HandleCapabilityActions()
{
    Console.WriteLine();
    var drone = PromptForDrone();
    if (drone == null) return;

    var hasCapabilities = DisplayCapabilities(drone);
    if (!hasCapabilities)
    {
        Console.WriteLine("This drone has no special capabilities.");
        return;
    }

    Console.Write("\nSelect capability action: ");
    var action = Console.ReadLine();

    try
    {
        switch (action)
        {
            case "1" when drone is IPhotoCapture photoCapture:
                HandlePhotoCapture(photoCapture, drone);
                break;
            case "2" when drone is ICargoCarrier cargoCarrier:
                HandleCargoLoad(cargoCarrier);
                break;
            case "3" when drone is ICargoCarrier cargoCarrier:
                HandleCargoUnload(cargoCarrier);
                break;
            default:
                Console.WriteLine("Invalid action or capability not supported.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

void HandleChargeBattery()
{
    Console.WriteLine();
    Console.Write("Enter drone ID (or 'all' for all drones): ");
    var chargeInput = Console.ReadLine();

    var chargeAmount = PromptForChargeAmount();
    if (chargeAmount == null) return;

    try
    {
        if (chargeInput?.ToLower() == "all")
        {
            Console.WriteLine();
            droneManager.ChargeAllDrones(chargeAmount.Value);
        }
        else if (int.TryParse(chargeInput, out int chargeDroneId))
        {
            var drone = droneManager.GetDroneById(chargeDroneId);
            if (drone == null)
            {
                Console.WriteLine("Drone not found.");
                return;
            }

            drone.ChargeBattery(chargeAmount.Value);
            Console.WriteLine($"Drone ID: {drone.Id}, New Battery Level: {drone.BatteryPercent}%");
        }
        else
        {
            Console.WriteLine("Invalid input.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error charging battery: {ex.Message}");
    }
}

Drone? PromptForDrone()
{
    Console.Write("Enter drone ID: ");
    if (!int.TryParse(Console.ReadLine(), out int droneId))
    {
        Console.WriteLine("Invalid ID.");
        return null;
    }

    var drone = droneManager.GetDroneById(droneId);
    if (drone == null)
    {
        Console.WriteLine("Drone not found.");
    }

    return drone;
}

(double lat, double lon)? PromptForCoordinates()
{
    Console.Write("Enter latitude: ");
    if (!double.TryParse(Console.ReadLine(), out double lat))
    {
        Console.WriteLine("Invalid latitude.");
        return null;
    }

    Console.Write("Enter longitude: ");
    if (!double.TryParse(Console.ReadLine(), out double lon))
    {
        Console.WriteLine("Invalid longitude.");
        return null;
    }

    return (lat, lon);
}

float? PromptForChargeAmount()
{
    Console.Write("Enter charge amount (%): ");
    if (!float.TryParse(Console.ReadLine(), out float chargeAmount))
    {
        Console.WriteLine("Invalid charge amount.");
        return null;
    }

    if (chargeAmount < 0)
    {
        Console.WriteLine("Charge amount cannot be negative.");
        return null;
    }

    return chargeAmount;
}

double? PromptForWeight()
{
    Console.Write("Enter weight to load (kg): ");
    if (!double.TryParse(Console.ReadLine(), out double kg))
    {
        Console.WriteLine("Invalid weight.");
        return null;
    }

    return kg;
}

bool DisplayCapabilities(Drone drone)
{
    Console.WriteLine($"\nCapabilities for {drone.Name}:");
    var hasCapabilities = false;

    if (drone is IPhotoCapture)
    {
        Console.WriteLine("- Photo Capture (1)");
        hasCapabilities = true;
    }
    if (drone is ICargoCarrier)
    {
        Console.WriteLine("- Cargo Operations (2=Load, 3=UnloadAll)");
        hasCapabilities = true;
    }

    return hasCapabilities;
}

void HandlePhotoCapture(IPhotoCapture photoCapture, Drone drone)
{
    photoCapture.TakePhoto();
    Console.WriteLine($"Photo taken! Total photos: {photoCapture.PhotoCount}. Battery: {drone.BatteryPercent}%");
}

void HandleCargoLoad(ICargoCarrier cargoCarrier)
{
    var kg = PromptForWeight();
    if (kg == null) return;

    if (cargoCarrier.Load(kg.Value))
    {
        Console.WriteLine($"Loaded {kg.Value:F2} kg. Current load: {cargoCarrier.CurrentLoadKg:F2}/{cargoCarrier.CapacityKg:F2} kg");
    }
    else
    {
        Console.WriteLine("Load failed. Check capacity or ensure drone is landed.");
    }
}

void HandleCargoUnload(ICargoCarrier cargoCarrier)
{
    cargoCarrier.UnloadAll();
    Console.WriteLine("All cargo unloaded.");
}
