using DroneFleet.Services;
using DroneFleet.Services.Creators;
using DroneFleet.Services.Interfaces;

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
            droneManager.ListDrones();
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
            droneManager.PreFlightCheckAll();
            break;

        case "4":
            break;
        case "5":
            break;
        case "6":
            break;
        case "7":
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
