namespace DroneFleet.Models.Interfaces;

internal interface ICargoCarrier
{
    /// <summary>
    /// Gets the maximum cargo capacity in kilograms.
    /// </summary>
    double CapacityKg { get; }

    /// <summary>
    /// Gets the current cargo load in kilograms.
    /// </summary>
    double CurrentLoadKg { get; }

    /// <summary>
    /// Attempts to load the specified weight onto the carrier.
    /// </summary>
    /// <param name="kg">The weight in kilograms to load.</param>
    /// <returns>True if the load was successful; otherwise, false.</returns>
    bool Load(double kg);

    /// <summary>
    /// Unloads all cargo from the carrier.
    /// </summary>
    void UnloadAll();
}
