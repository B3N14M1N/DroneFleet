using DroneFleet.Models;

namespace DroneFleet.Services.Interfaces;

internal interface IDroneRepository
{
    IReadOnlyList<Drone> GetAll();

    void Add(Drone drone);

    Drone? GetById(int id);

    bool Remove(int id);

    IEnumerable<TCapability> GetByCapability<TCapability>();
}
