using System.Text;
using DroneFleet.App.ConsoleApp;

namespace DroneFleet.App.ConsoleApp.Commands;

/// <summary>
/// Displays the list of available commands.
/// </summary>
internal sealed class HelpCommand : IConsoleCommand
{
    private readonly CommandRegistry _registry;

    public HelpCommand(CommandRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public string Name => "help";

    public string Description => "Displays this help message.";

    public string Usage => "help";

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Available commands:");

        foreach (var command in _registry.Commands)
        {
            builder.Append("  ")
                .Append(command.Name.PadRight(10))
                .Append(" - ")
                .AppendLine(command.Description);

            if (!string.IsNullOrWhiteSpace(command.Usage))
            {
                builder.Append("     Usage: ")
                    .AppendLine(command.Usage);
            }
        }

        context.Output.Write(builder.ToString());
        return ValueTask.CompletedTask;
    }
}
