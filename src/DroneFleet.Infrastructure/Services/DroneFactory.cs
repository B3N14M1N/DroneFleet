using DroneFleet.Domain.Contracts;
using DroneFleet.Domain.Models;
using DroneFleet.Infrastructure.Services.Interfaces;

namespace DroneFleet.Infrastructure.Services;

public class DroneFactory : IDroneFactory
{
    private static int _id = 0;

    private static int NextId() => ++_id;

    /// <inheritdoc/>
    public Drone Create<TDrone>(string name, DroneCreationOptions? options = null)
        where TDrone : Drone
    {
        var id = NextId();

        return typeof(TDrone) switch
        {
            var t when t == typeof(DeliveryDrone) =>
                new DeliveryDrone()
                {
                    Id = id,
                    Name = name,
                    CapacityKg = options?.CapacityKg ?? 5f
                },

            var t when t == typeof(SurveyDrone) => 
                new SurveyDrone()
                {
                    Id = id,
                    Name = name
                },
            var t when t == typeof(RacingDrone) => 
                new RacingDrone()
                {
                    Id = id,
                    Name = name
                },
            _ => throw new InvalidOperationException($"Unsupported drone type {typeof(TDrone).Name}")
        };
    }
}
