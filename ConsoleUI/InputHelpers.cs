using DroneFleet.Models;
using DroneFleet.Services.Interfaces;

namespace DroneFleet.ConsoleUI;

/// <summary>
/// Provides helper methods for reading and validating user input from the console.
/// </summary>
internal static class InputHelpers
{
    /// <summary>
    /// Prompts the user to enter a drone ID and retrieves the corresponding drone.
    /// Retries until valid input or user cancels with empty input.
    /// </summary>
    /// <param name="droneManager">The drone manager to query.</param>
    /// <returns>The drone if found; otherwise, null.</returns>
    public static Drone? PromptForDrone(IDroneManager droneManager)
    {
        Console.WriteLine("Enter drone ID (or press Enter to cancel): ");
        while (true)
        {
            Console.Write(">>> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (!int.TryParse(input, out int droneId))
            {
                PrintError("Invalid ID. Please enter a number.");
                continue;
            }

            var drone = droneManager.GetDroneById(droneId);
            if (drone == null)
            {
                PrintError($"Drone with ID {droneId} not found.");
                continue;
            }

            return drone;
        }
    }

    /// <summary>
    /// Prompts the user to enter latitude and longitude coordinates.
    /// Retries until valid input or user cancels with empty input.
    /// </summary>
    /// <returns>A tuple containing latitude and longitude if valid; otherwise, null.</returns>
    public static (double lat, double lon)? PromptForCoordinates()
    {
        double lat;
        Console.WriteLine("Enter latitude (or press Enter to cancel): ");
        while (true)
        {
            Console.Write(">>> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (!double.TryParse(input, out lat))
            {
                PrintError("Invalid latitude. Please enter a valid number.");
                continue;
            }

            break;
        }

        double lon;
        Console.WriteLine("Enter longitude (or press Enter to cancel): ");
        while (true)
        {
            Console.Write(">>> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (!double.TryParse(input, out lon))
            {
                PrintError("Invalid longitude. Please enter a valid number.");
                continue;
            }

            break;
        }

        return (lat, lon);
    }

    /// <summary>
    /// Prompts the user to select from numbered options.
    /// Retries until valid input or user cancels with empty input.
    /// </summary>
    /// <param name="prompt">The prompt message to display.</param>
    /// <param name="maxOption">The maximum option number (1-based).</param>
    /// <returns>The selected option number (1-based), or null if cancelled.</returns>
    public static int? PromptForOption(string prompt, int maxOption, int minOption = 1)
    {
        Console.WriteLine($"{prompt} (or press Enter to cancel): ");
        while (true)
        {
            Console.Write(">>> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (!int.TryParse(input, out int option))
            {
                PrintError("Invalid input. Please enter a number.");
                continue;
            }

            if (option < minOption || option > maxOption)
            {
                PrintError($"Please enter a number between {minOption} and {maxOption}.");
                continue;
            }

            return option;
        }
    }

    /// <summary>
    /// Prompts the user to enter a double value with optional default.
    /// Retries until valid input or user accepts default with empty input.
    /// </summary>
    /// <param name="prompt">The prompt message to display.</param>
    /// <param name="defaultValue">The default value if user presses Enter.</param>
    /// <param name="min">Optional minimum value.</param>
    /// <returns>The entered value or default value.</returns>
    public static double? PromptForDouble(string prompt, double? defaultValue = null, double? min = null, double? max = null)
    {
        Console.WriteLine($"{prompt}" + (defaultValue.HasValue ? $"(default: {defaultValue}):" : ""));
        while (true)
        {
            Console.Write(">>> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                if (defaultValue.HasValue)
                    return defaultValue;

                return null;
            }

            if (!double.TryParse(input, out double value))
            {
                PrintError("Invalid input. Please enter a valid number.");
                continue;
            }

            if (min.HasValue && value < min.Value)
            {
                PrintError($"Value must be at least {min.Value}.");
                continue;
            }

            if (max.HasValue && value > max.Value)
            {
                PrintError($"Value must be at most {max.Value}.");
                continue;
            }

            return value;
        }
    }

    /// <summary>
    /// Prompts the user to enter a string value with optional default.
    /// </summary>
    /// <param name="prompt">The prompt message to display.</param>
    /// <param name="defaultValue">The default value if user presses Enter.</param>
    /// <returns>The entered string or default value.</returns>
    public static string PromptForString(string prompt, string defaultValue = "")
    {
        Console.WriteLine($"{prompt} " + (string.IsNullOrEmpty(defaultValue) ? "" : $"(default: {defaultValue}): "));
        Console.Write(">>> ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return defaultValue;
        }

        return input;
    }

    /// <summary>
    /// Prints an error message in red color.
    /// </summary>
    /// <param name="msg">The error message to display.</param>
    public static void PrintError(string msg)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ForegroundColor = prev;
    }

    /// <summary>
    /// Prints a success message in green color.
    /// </summary>
    /// <param name="msg">The success message to display.</param>
    public static void PrintSuccess(string msg)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(msg);
        Console.ForegroundColor = prev;
    }
}
