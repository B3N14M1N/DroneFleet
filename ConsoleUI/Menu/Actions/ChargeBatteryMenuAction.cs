using DroneFleet.Services.Interfaces;

namespace DroneFleet.ConsoleUI.Menu.Actions;

internal sealed class ChargeBatteryMenuAction(IDroneManager droneManager) : IMenuAction
{
    private readonly IDroneManager _droneManager = droneManager;

    public string Label => "Charge battery";

    public string Description => "Charge one or all drones";

    public MenuActionOutcome Execute()
    {
        ArgumentNullException.ThrowIfNull(_droneManager);

        Console.WriteLine();
        var chargeInput = InputHelpers.PromptForString("Enter drone ID (or 'all' for all drones, or press Enter to cancel): ");
        if (string.IsNullOrEmpty(chargeInput))
        {
            return MenuActionOutcome.Continue;
        }

        var chargeAmount = InputHelpers.PromptForDouble("Enter % charge amount (or press Enter to cancel): ", min: 0, max: 100);
        if (chargeAmount == null)
        {
            return MenuActionOutcome.Continue;
        }

        try
        {
            if (chargeInput.Equals("all", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine();
                var results = _droneManager.ChargeAllDrones(chargeAmount.Value).ToList();

                if (results.Count == 0)
                {
                    Console.WriteLine("No drones available.");
                    return MenuActionOutcome.Continue;
                }

                foreach (var result in results)
                {
                    if (result.Success)
                    {
                        InputHelpers.PrintSuccess($"Drone ID: {result.Drone.Id}, New Battery Level: {result.BatteryLevel}%");
                    }
                    else
                    {
                        var message = result.ErrorMessage ?? "Charging failed.";
                        InputHelpers.PrintError($"Drone ID: {result.Drone.Id}, Charging failed: {message}");
                    }
                }
            }
            else if (int.TryParse(chargeInput, out int chargeDroneId))
            {
                var drone = _droneManager.GetDroneById(chargeDroneId);
                if (drone == null)
                {
                    InputHelpers.PrintError("Drone not found.");
                    return MenuActionOutcome.Continue;
                }

                drone.ChargeBattery(chargeAmount.Value);
                InputHelpers.PrintSuccess($"Drone ID: {drone.Id}, New Battery Level: {drone.BatteryPercent}%");
            }
            else
            {
                InputHelpers.PrintError("Invalid input.");
            }
        }
        catch (Exception ex)
        {
            InputHelpers.PrintError($"Error charging battery: {ex.Message}");
        }

        return MenuActionOutcome.Continue;
    }
}
