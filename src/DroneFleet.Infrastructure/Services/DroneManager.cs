using DroneFleet.Domain.Contracts;
using DroneFleet.Domain.Models;
using DroneFleet.Infrastructure.Services.Interfaces;

namespace DroneFleet.Infrastructure.Services;

public class DroneManager(IDroneRepository repository) : IDroneManager
{
    private readonly IDroneRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public IEnumerable<Drone> Drones => _repository.GetAll();

    /// <inheritdoc/>
    public void AddDrone(Drone drone)
    {
        _repository.Add(drone);
    }

    /// <inheritdoc/>
    public IEnumerable<DroneChargeResult> ChargeAllDrones(double chargeAmount)
    {
        foreach (var drone in _repository.GetAll())
        {
            DroneChargeResult result;
            try
            {
                drone.ChargeBattery(chargeAmount);
                result = new DroneChargeResult(drone, true, drone.BatteryPercent, null);
            }
            catch (Exception ex)
            {
                result = new DroneChargeResult(drone, false, null, ex.Message);
            }

            yield return result;
        }
    }

    /// <inheritdoc/>
    public Drone? GetDroneById(int id)
    {
        return _repository.GetById(id);
    }

    /// <inheritdoc/>
    public IEnumerable<DroneSelfTestResult> PreFlightCheckAll()
    {
        foreach (var drone in _repository.GetAll())
        {
            DroneSelfTestResult outcome;
            try
            {
                bool passed = drone.RunSelfTest();
                outcome = new DroneSelfTestResult(drone, passed, passed ? null : "Self-test reported failure.");
            }
            catch (Exception ex)
            {
                outcome = new DroneSelfTestResult(drone, false, ex.Message);
            }

            yield return outcome;
        }
    }

    /// <inheritdoc/>
    public bool RemoveDrone(int id)
    {
        return _repository.Remove(id);
    }
}
