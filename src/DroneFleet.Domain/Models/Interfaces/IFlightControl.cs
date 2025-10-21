namespace DroneFleet.Domain.Models.Interfaces;

public interface IFlightControl
{
    /// <summary>
    /// Initiates the drone's takeoff sequence.
    /// </summary>
    void TakeOff();

    /// <summary>
    /// Initiates the drone's landing sequence.
    /// </summary>
    void Land();
}
