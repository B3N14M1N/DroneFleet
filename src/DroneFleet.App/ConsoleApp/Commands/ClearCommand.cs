namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Clears the console screen.
/// </summary>
internal sealed class ClearCommand : IConsoleCommand
{
    public string Name => "clear";

    public string Description => "Clears the console screen.";

    public string Usage => "clear";

    public string HelpText => "clear | cls" + Environment.NewLine + "Clears the visible console output.";

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        Console.Clear();
        return ValueTask.CompletedTask;
    }
}
