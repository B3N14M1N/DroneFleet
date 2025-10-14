using DroneFleet.Models;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.Services;

internal class DroneManager : IDroneManager
{
    private readonly List<Drone> _drones = [];

    public IEnumerable<Drone> Drones => _drones.AsReadOnly();

    /// <inheritdoc/>
    public void AddDrone(Drone drone)
    {
        if (drone == null)
        {
            throw new ArgumentNullException(nameof(drone), "Drone cannot be null.");
        }
        _drones.Add(drone);
    }

    /// <inheritdoc/>
    public Drone? GetDroneById(int id)
    {
        return _drones.FirstOrDefault(d => d.Id == id);
    }

    /// <inheritdoc/>
    public bool RemoveDrone(int id)
    {
        var drone = GetDroneById(id);
        if (drone != null)
        {
            return _drones.Remove(drone);
        }
        return false;
    }

    /// <inheritdoc/>
    public void ListDrones()
    {
        if (_drones.Count == 0)
        {
            Console.WriteLine("No drones available.");
            return;
        }
        foreach (var drone in _drones)
        {
            Console.WriteLine(drone);
        }
    }

    /// <inheritdoc/>
    public void ListDronesByCategory(Dictionary<string, Type> categories)
    {
        if (_drones.Count == 0)
        {
            Console.WriteLine("No drones available.");
            return;
        }

        foreach (var (categoryName, droneType) in categories)
        {
            var dronesInCategory = _drones.Where(d => d.GetType() == droneType).ToList();

            if (dronesInCategory.Count != 0)
            {
                Console.WriteLine($"=== {categoryName} ===");
                foreach (var drone in dronesInCategory)
                {
                    Console.WriteLine(drone);
                }
                Console.WriteLine();
            }
        }
    }

    /// <inheritdoc/>
    public void PreFlightCheckAll()
    {
        if (_drones.Count == 0)
        {
            Console.WriteLine("No drones available.");
            return;
        }

        foreach (var drone in _drones)
        {
            try
            {
                bool result = drone.RunSelfTest();
                Console.WriteLine($"Drone ID: {drone.Id}, Self-test result: {(result ? "Passed" : "Failed")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Drone ID: {drone.Id}, Self-test failed with error: {ex.Message}");
            }
        }
    }

    /// <inheritdoc/>
    public void ChargeAllDrones(float chargeAmount)
    {
        if (_drones.Count == 0)
        {
            Console.WriteLine("No drones available.");
            return;
        }

        foreach (var drone in _drones)
        {
            try
            {
                drone.ChargeBattery(chargeAmount);
                Console.WriteLine($"Drone ID: {drone.Id}, New Battery Level: {drone.BatteryPercent}%");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Drone ID: {drone.Id}, Charging failed: {ex.Message}");
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<TCapability> GetDronesByCapability<TCapability>()
    {
        return _drones.OfType<TCapability>();
    }
}
