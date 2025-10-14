using DroneFleet.Models;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.Services;

/// <summary>
/// Provides lookup and discovery for capability action handlers.
/// </summary>
internal sealed class CapabilityRegistry(IEnumerable<ICapabilityActionHandler> handlers)
{
    private readonly List<ICapabilityActionHandler> _handlers = handlers.ToList();
    private readonly Dictionary<string, ICapabilityActionHandler> _handlersByKey =
        handlers.ToDictionary(handler => handler.Key, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Enumerates all registered capability handlers.
    /// </summary>
    public IEnumerable<ICapabilityActionHandler> All => _handlers;

    /// <summary>
    /// Retrieves handlers that support the provided drone instance.
    /// </summary>
    /// <param name="drone">The drone being inspected.</param>
    /// <returns>Handlers whose actions can be executed for the drone.</returns>
    public IEnumerable<ICapabilityActionHandler> GetHandlersFor(Drone drone) =>
        _handlers.Where(handler => handler.Supports(drone));

    /// <summary>
    /// Attempts to retrieve a handler by its unique key.
    /// </summary>
    /// <param name="key">The capability handler key.</param>
    /// <param name="handler">The handler when found.</param>
    /// <returns>True if the handler is present; otherwise, false.</returns>
    public bool TryGet(string key, out ICapabilityActionHandler? handler) =>
        _handlersByKey.TryGetValue(key, out handler);
}
