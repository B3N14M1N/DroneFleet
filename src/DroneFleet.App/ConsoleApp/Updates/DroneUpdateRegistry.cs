using DroneFleet.Domain.Models;

namespace DroneFleet.App.ConsoleApp.Updates;

/// <summary>
/// Maintains a lookup between update keywords and handlers.
/// </summary>
internal sealed class DroneUpdateRegistry
{
    private readonly Dictionary<string, List<IDroneUpdateHandler>> _handlers = new(StringComparer.OrdinalIgnoreCase);

    public void Register(IDroneUpdateHandler handler, params string[] aliases)
    {
        ArgumentNullException.ThrowIfNull(handler);
        AddHandler(handler.Keyword, handler);

        if (aliases is null)
        {
            return;
        }

        foreach (var alias in aliases)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                continue;
            }

            AddHandler(alias, handler);
        }
    }

    public IDroneUpdateHandler? Resolve(string keyword, Drone drone)
    {
        if (!_handlers.TryGetValue(keyword, out var candidates))
        {
            return null;
        }

        return candidates.FirstOrDefault(handler => handler.Supports(drone));
    }

    public IReadOnlyList<string> GetSupportedKeywords(Drone drone)
    {
        return _handlers
            .Where(pair => pair.Value.Any(handler => handler.Supports(drone)))
            .Select(pair => pair.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(key => key)
            .ToArray();
    }

    /// <summary>
    /// Retrieves all registered update keywords irrespective of drone type.
    /// </summary>
    public IReadOnlyList<string> GetAllKeywords()
    {
        return _handlers.Keys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(k => k)
            .ToArray();
    }

    private void AddHandler(string keyword, IDroneUpdateHandler handler)
    {
        if (!_handlers.TryGetValue(keyword, out var list))
        {
            list = new List<IDroneUpdateHandler>();
            _handlers[keyword] = list;
        }

        list.Add(handler);
    }
}
