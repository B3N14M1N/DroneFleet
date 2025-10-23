using DroneFleet.App.ConsoleApp;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Terminates the application.
/// </summary>
internal sealed class ExitCommand : IConsoleCommand
{
    public string Name => "exit";

    public string Description => "Exits the application.";

    public string Usage => "exit";

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        context.RequestExit();
        return ValueTask.CompletedTask;
    }
}
