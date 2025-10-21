namespace DroneFleet.Domain.Models.Interfaces;

public interface ISelfTest
{
    /// <summary>
    /// Runs a self-test on the drone's systems.
    /// </summary>
    /// <returns>True if the self-test passes; otherwise, false.</returns>
    bool RunSelfTest();
}
