using DroneFleet.Infrastructure.Services.Interfaces;

namespace DroneFleet.Infrastructure.Services;

/// <summary>
/// Registry for managing and retrieving drone creators by key.
/// </summary>
public sealed class DroneCreationRegistry(IEnumerable<IDroneCreator> creators)
{
    private readonly Dictionary<string, IDroneCreator> _byKey = creators.ToDictionary(c => c.Key, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets all registered drone creators.
    /// </summary>
    public IEnumerable<IDroneCreator> All => _byKey.Values;

    /// <summary>
    /// Attempts to retrieve a drone creator by its unique key.
    /// </summary>
    /// <param name="key">The unique key identifying the drone creator.</param>
    /// <param name="creator">The drone creator instance, if found.</param>
    /// <returns>True if the creator was found; otherwise, false.</returns>
    public bool TryGet(string key, out IDroneCreator creator) => _byKey.TryGetValue(key, out creator);
}
