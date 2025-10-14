using DroneFleet.Models;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.Services;

internal class DroneRepository : IDroneRepository
{
    private readonly List<Drone> _drones = [];

    public IReadOnlyList<Drone> GetAll() => _drones;

    public void Add(Drone drone)
    {
        ArgumentNullException.ThrowIfNull(drone);
        _drones.Add(drone);
    }

    public Drone? GetById(int id)
    {
        return _drones.FirstOrDefault(d => d.Id == id);
    }

    public bool Remove(int id)
    {
        var drone = GetById(id);
        return drone != null && _drones.Remove(drone);
    }

    public IEnumerable<TCapability> GetByCapability<TCapability>()
    {
        return _drones.OfType<TCapability>();
    }
}
