using DroneFleet.Infrastructure.Services.Interfaces;

namespace DroneFleet.App.ConsoleUI.Menu.Actions;

internal sealed class PreFlightCheckMenuAction(IDroneManager droneManager) : IMenuAction
{
    private readonly IDroneManager _droneManager = droneManager;

    public string Label => "Pre-flight check (all)";

    /// <inheritdoc/>
    public MenuActionOutcome Execute()
    {
        ArgumentNullException.ThrowIfNull(_droneManager);

        Console.WriteLine();
        Console.WriteLine("Running pre-flight checks on all drones:");
        try
        {
            var results = _droneManager.PreFlightCheckAll().ToList();

            if (results.Count == 0)
            {
                Console.WriteLine("No drones available.");
                return MenuActionOutcome.Continue;
            }

            foreach (var result in results)
            {
                if (result.Passed)
                {
                    InputHelpers.PrintSuccess($"Drone ID: {result.Drone.Id}, Self-test passed.");
                }
                else
                {
                    var message = result.ErrorMessage ?? "Self-test failed.";
                    InputHelpers.PrintError($"Drone ID: {result.Drone.Id}, Self-test failed: {message}");
                }
            }
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error during pre-flight check: {ex.Message}");
        }

        return MenuActionOutcome.Continue;
    }
}
