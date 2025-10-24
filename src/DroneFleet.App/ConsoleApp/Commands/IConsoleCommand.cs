namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Represents a console command that can be executed from the shell.
/// </summary>
internal interface IConsoleCommand
{
    /// <summary>
    /// Gets the command name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the short command description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets an optional usage hint shown in the help screen.
    /// </summary>
    string Usage { get; }

    /// <summary>
    /// Gets detailed help text for the command.
    /// </summary>
    string HelpText { get; }

    /// <summary>
    /// Executes the command using the provided context and arguments.
    /// </summary>
    ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken);
}
