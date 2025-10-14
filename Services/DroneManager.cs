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
    public void PreFlightCheckAll()
    {
        foreach (var drone in _drones)
        {
            bool result = drone.RunSelfTest();
            Console.WriteLine($"Drone ID: {drone.Id}, Self-test result: {(result ? "Passed" : "Failed")}");
        }
    }

    /// <inheritdoc/>
    public void ChargeAllDrones(float chargeAmount)
    {
        foreach (var drone in _drones)
        {
            drone.ChargeBattery(chargeAmount);
            Console.WriteLine($"Drone ID: {drone.Id}, New Battery Level: {drone.BatteryPercent}%");
        }
    }

    /// <inheritdoc/>
    public IEnumerable<TCapability> GetDronesByCapability<TCapability>()
    {
        return _drones.OfType<TCapability>();
    }
}
