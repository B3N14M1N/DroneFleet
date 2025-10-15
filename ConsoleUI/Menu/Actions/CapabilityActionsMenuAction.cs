using DroneFleet.Services;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.ConsoleUI.Menu.Actions;

internal sealed class CapabilityActionsMenuAction(IDroneManager droneManager, CapabilityRegistry capabilityRegistry) : IMenuAction
{
    private readonly IDroneManager _droneManager = droneManager;
    private readonly CapabilityRegistry _capabilityRegistry = capabilityRegistry;

    public string Label => "Capability actions";

    /// <inheritdoc/>
    public MenuActionOutcome Execute()
    {
        ArgumentNullException.ThrowIfNull(_droneManager);
        ArgumentNullException.ThrowIfNull(_capabilityRegistry);

        Console.WriteLine();
        var drone = InputHelpers.PromptForDrone(_droneManager);
        if (drone == null)
        {
            return MenuActionOutcome.Continue;
        }

        var handlers = _capabilityRegistry
            .GetHandlersFor(drone)
            .ToList();

        if (handlers.Count == 0)
        {
            InputHelpers.PrintError("This drone has no special capabilities.");
            return MenuActionOutcome.Continue;
        }

        Console.WriteLine($"\nCapabilities for {drone.Name}:");
        for (int index = 0; index < handlers.Count; index++)
        {
            Console.WriteLine($"{index + 1}. {handlers[index].DisplayName}");
        }

        var selection = InputHelpers.PromptForOption("\nSelect capability action (or press Enter to cancel): ", handlers.Count);
        if (selection == null)
        {
            return MenuActionOutcome.Continue;
        }

        var handler = handlers[selection.Value - 1];

        try
        {
            handler.Execute(drone);
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error: {ex.Message}");
        }

        return MenuActionOutcome.Continue;
    }
}
