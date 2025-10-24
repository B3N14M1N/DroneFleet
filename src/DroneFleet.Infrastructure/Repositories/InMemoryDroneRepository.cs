using DroneFleet.Domain.Common;
using DroneFleet.Domain.Models;
using DroneFleet.Domain.Repositories;

namespace DroneFleet.Infrastructure.Repositories;

/// <summary>
/// In-memory repository tailored for drone entities.
/// </summary>
public sealed class InMemoryDroneRepository(int startingId) : IDroneRepository
{
    private readonly InMemoryRepository<Drone, int> _repository = new(drone => drone.Id);
    private int _currentId = startingId;

    public InMemoryDroneRepository() : this(0)
    {
    }

    public Result<Drone> Add(Drone entity)
    {
        var result = _repository.Add(entity);
        if (result.IsSuccess)
        {
            UpdateCurrentId(entity.Id);
        }

        return result;
    }

    public Result<Drone> Upsert(Drone entity)
    {
        var result = _repository.Upsert(entity);
        if (result.IsSuccess)
        {
            UpdateCurrentId(entity.Id);
        }

        return result;
    }

    public Result<Drone> GetById(int key) => _repository.GetById(key);

    public Result Remove(int key) => _repository.Remove(key);

    public IReadOnlyCollection<Drone> List() => _repository.List();

    public int NextId() => Interlocked.Increment(ref _currentId);

    private void UpdateCurrentId(int id)
    {
        while (true)
        {
            var current = Volatile.Read(ref _currentId);
            if (id <= current)
            {
                return;
            }

            var original = Interlocked.CompareExchange(ref _currentId, id, current);
            if (original == current)
            {
                return;
            }
        }
    }
}
