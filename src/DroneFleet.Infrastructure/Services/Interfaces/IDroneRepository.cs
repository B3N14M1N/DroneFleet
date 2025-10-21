using DroneFleet.Domain.Models;

namespace DroneFleet.Infrastructure.Services.Interfaces;

public interface IDroneRepository
{
    IReadOnlyList<Drone> GetAll();

    void Add(Drone drone);

    Drone? GetById(int id);

    bool Remove(int id);

    IEnumerable<TCapability> GetByCapability<TCapability>();
}
