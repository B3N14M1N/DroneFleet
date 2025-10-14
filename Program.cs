using DroneFleet.Services;
using DroneFleet.Services.Creators;
using DroneFleet.Services.Interfaces;
using DroneFleet.Models.Interfaces;

IDroneFactory droneFactory = new DroneFactory();
IDroneManager droneManager = new DroneManager();

var registry = new DroneCreationRegistry(
[
    new DeliveryDroneCreator(),
    new SurveyDroneCreator(),
    new RacingDroneCreator(),
]);

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
            Console.WriteLine();
            Console.WriteLine("Listing all drones:");
            try
            {
                droneManager.ListDrones();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing drones: {ex.Message}");
            }
            break;

        case "2":
            Console.WriteLine();
            Console.WriteLine("Available types:");
            foreach (var c in registry.All)
                Console.WriteLine($"- {c.Key} ({c.DisplayName})");

            Console.Write("Enter drone type: ");
            var key = Console.ReadLine();

            if (!registry.TryGet(key ?? "", out var creator))
            {
                Console.WriteLine("Unknown type.");
                break;
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
            break;

        case "3":
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
            break;

        case "4":
            Console.WriteLine();
            Console.Write("Enter drone ID: ");
            if (int.TryParse(Console.ReadLine(), out int droneId))
            {
                var drone = droneManager.GetDroneById(droneId);
                if (drone == null)
                {
                    Console.WriteLine("Drone not found.");
                    break;
                }

                Console.WriteLine($"Current status: {(drone.IsAirborne ? "Airborne" : "Landed")}");
                Console.Write("Action (1=TakeOff, 2=Land): ");
                var action = Console.ReadLine();

                try
                {
                    if (action == "1")
                    {
                        drone.TakeOff();
                        Console.WriteLine($"{drone.Name} took off. Battery: {drone.BatteryPercent}%");
                    }
                    else if (action == "2")
                    {
                        drone.Land();
                        Console.WriteLine($"{drone.Name} landed. Battery: {drone.BatteryPercent}%");
                    }
                    else
                    {
                        Console.WriteLine("Invalid action.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid ID.");
            }
            break;

        case "5":
            Console.WriteLine();
            Console.Write("Enter drone ID: ");
            if (int.TryParse(Console.ReadLine(), out int waypointDroneId))
            {
                var drone = droneManager.GetDroneById(waypointDroneId);
                if (drone == null)
                {
                    Console.WriteLine("Drone not found.");
                    break;
                }

                if (drone is INavigable navigableDrone)
                {
                    Console.Write("Enter latitude: ");
                    if (double.TryParse(Console.ReadLine(), out double lat))
                    {
                        Console.Write("Enter longitude: ");
                        if (double.TryParse(Console.ReadLine(), out double lon))
                        {
                            try
                            {
                                navigableDrone.SetWaypoint(lat, lon);
                                Console.WriteLine($"Waypoint set to ({lat}, {lon}) for {drone.Name}. Battery: {drone.BatteryPercent}%");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error setting waypoint: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid longitude.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid latitude.");
                    }
                }
                else
                {
                    Console.WriteLine($"{drone.Name} does not support navigation.");
                }
            }
            else
            {
                Console.WriteLine("Invalid ID.");
            }
            break;

        case "6":
            Console.WriteLine();
            Console.Write("Enter drone ID: ");
            if (int.TryParse(Console.ReadLine(), out int capabilityDroneId))
            {
                var drone = droneManager.GetDroneById(capabilityDroneId);
                if (drone == null)
                {
                    Console.WriteLine("Drone not found.");
                    break;
                }

                Console.WriteLine($"\nCapabilities for {drone.Name}:");
                var hasCapabilities = false;

                if (drone is IPhotoCapture)
                {
                    Console.WriteLine("- Photo Capture (1=Survey)");
                    hasCapabilities = true;
                }
                if (drone is ICargoCarrier)
                {
                    Console.WriteLine("- Cargo Operations (2=Load, 3=UnloadAll)");
                    hasCapabilities = true;
                }

                if (!hasCapabilities)
                {
                    Console.WriteLine("This drone has no special capabilities.");
                    break;
                }

                Console.Write("\nSelect capability action: ");
                var capabilityAction = Console.ReadLine();

                try
                {
                    if (capabilityAction == "1" && drone is IPhotoCapture photoCapture)
                    {
                        photoCapture.TakePhoto();
                        Console.WriteLine($"Photo taken! Total photos: {photoCapture.PhotoCount}. Battery: {drone.BatteryPercent}%");
                    }
                    else if (capabilityAction == "2" && drone is ICargoCarrier cargoCarrier)
                    {
                        Console.Write("Enter weight to load (kg): ");
                        if (double.TryParse(Console.ReadLine(), out double kg))
                        {
                            if (cargoCarrier.Load(kg))
                            {
                                Console.WriteLine($"Loaded {kg:F2} kg. Current load: {cargoCarrier.CurrentLoadKg:F2}/{cargoCarrier.CapacityKg:F2} kg");
                            }
                            else
                            {
                                Console.WriteLine("Load failed. Check capacity or ensure drone is landed.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid weight.");
                        }
                    }
                    else if (capabilityAction == "3" && drone is ICargoCarrier cargoCarrier2)
                    {
                        cargoCarrier2.UnloadAll();
                        Console.WriteLine("All cargo unloaded.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid action or capability not supported.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid ID.");
            }
            break;

        case "7":
            Console.WriteLine();
            Console.Write("Enter drone ID (or 'all' for all drones): ");
            var chargeInput = Console.ReadLine();

            Console.Write("Enter charge amount (%): ");
            if (float.TryParse(Console.ReadLine(), out float chargeAmount))
            {
                if (chargeAmount < 0)
                {
                    Console.WriteLine("Charge amount cannot be negative.");
                    break;
                }

                try
                {
                    if (chargeInput?.ToLower() == "all")
                    {
                        Console.WriteLine();
                        droneManager.ChargeAllDrones(chargeAmount);
                    }
                    else if (int.TryParse(chargeInput, out int chargeDroneId))
                    {
                        var drone = droneManager.GetDroneById(chargeDroneId);
                        if (drone == null)
                        {
                            Console.WriteLine("Drone not found.");
                            break;
                        }

                        drone.ChargeBattery(chargeAmount);
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
            else
            {
                Console.WriteLine("Invalid charge amount.");
            }
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
