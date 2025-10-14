using DroneFleet.Contracts;
using DroneFleet.Models;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.Services.Creators;

internal sealed class DeliveryDroneCreator : IDroneCreator
{
    public string Key => "delivery";
    public string DisplayName => "Delivery Drone";

    /// <inheritdoc/>
    public Drone CreateInteractive(IDroneFactory factory)
    {
        Console.Write("Enter name: ");
        var name = Console.ReadLine() ?? "Unnamed Delivery Drone";

        double capacity;
        while (true)
        {
            Console.Write("Capacity kg (>= 0, default 5): ");
            var txt = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(txt)) { capacity = 5.0; break; }
            if (double.TryParse(txt, out capacity) && capacity >= 0) break;
            PrintError("Invalid number. Try again.");
        }

        var opts = new DroneCreationOptions { CapacityKg = capacity };
        return factory.Create<DeliveryDrone>(name, opts);               
    }

    static void PrintError(string msg)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ForegroundColor = prev;
    }
}
