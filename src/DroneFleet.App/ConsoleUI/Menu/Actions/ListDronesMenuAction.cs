using DroneFleet.Domain.Models;
using DroneFleet.Infrastructure.Services.Interfaces;

namespace DroneFleet.App.ConsoleUI.Menu.Actions;

internal sealed class ListDronesMenuAction(IDroneManager droneManager, Dictionary<string, Type> categories) : IMenuAction
{
    private readonly IDroneManager _droneManager = droneManager;
    private readonly Dictionary<string, Type> _categories = categories;

    public string Label => "List drones";

    /// <inheritdoc/>
    public MenuActionOutcome Execute()
    {
        ArgumentNullException.ThrowIfNull(_droneManager);
        ArgumentNullException.ThrowIfNull(_categories);

        Console.WriteLine();
        Console.WriteLine("Listing all drones:");

        try
        {
            var drones = _droneManager.Drones.ToList();

            if (drones.Count == 0)
            {
                Console.WriteLine("No drones available.");
                return MenuActionOutcome.Continue;
            }

            foreach (var (categoryName, droneType) in _categories)
            {
                var dronesInCategory = drones
                    .Where(d => d.GetType() == droneType)
                    .Cast<Drone>()
                    .ToList();

                if (dronesInCategory.Count == 0)
                {
                    continue;
                }

                DroneDisplayFormatter.PrintCategory(categoryName, dronesInCategory);
            }

            var categorizedTypes = _categories.Values.ToHashSet();
            var uncategorized = drones
                .Where(d => !categorizedTypes.Contains(d.GetType()))
                .Cast<Drone>()
                .ToList();

            if (uncategorized.Count > 0)
            {
                DroneDisplayFormatter.PrintCategory("Other Drones", uncategorized);
            }
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error listing drones: {ex.Message}");
        }

        return MenuActionOutcome.Continue;
    }
}
