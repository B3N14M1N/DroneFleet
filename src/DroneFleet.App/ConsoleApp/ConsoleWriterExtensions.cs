using System;
using System.IO;

namespace DroneFleet.App.ConsoleApp;

/// <summary>
/// Provides helpers for writing coloured output to the console when available.
/// </summary>
internal static class ConsoleWriterExtensions
{
    public static void WriteErrorLine(this TextWriter writer, string message)
    {
        WriteLineWithColour(writer, message, ConsoleColor.Red);
    }

    public static void WriteWarningLine(this TextWriter writer, string message)
    {
        WriteLineWithColour(writer, message, ConsoleColor.Yellow);
    }

    public static void WriteSuccessLine(this TextWriter writer, string message)
    {
        WriteLineWithColour(writer, message, ConsoleColor.Green);
    }

    public static void WriteInfoLine(this TextWriter writer, string message)
    {
        WriteLineWithColour(writer, message, ConsoleColor.Cyan);
    }

    private static void WriteLineWithColour(TextWriter writer, string message, ConsoleColor colour)
    {
        ArgumentNullException.ThrowIfNull(writer);
        message ??= string.Empty;

        if (ReferenceEquals(writer, Console.Out) && !Console.IsOutputRedirected)
        {
            var previous = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = colour;
                writer.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = previous;
            }
        }
        else
        {
            writer.WriteLine(message);
        }
    }
}
