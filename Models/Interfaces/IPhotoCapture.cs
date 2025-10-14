namespace DroneFleet.Models.Interfaces;

internal interface IPhotoCapture
{
    /// <summary>
    /// Captures a photo using the drone's camera.
    /// </summary>
    void TakePhoto();

    /// <summary>
    /// Gets the total number of photos taken.
    /// </summary>
    int PhotoCount { get; }
}
