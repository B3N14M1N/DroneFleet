using System.Text;

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

    public string Description => "Displays this help message or detailed info for a specific command.";

    public string Usage => "help [<command>]";

    public string HelpText =>
        "help [<command>]" + Environment.NewLine +
        "Without arguments shows all commands." + Environment.NewLine +
        "Provide a command name to see detailed help for that command (e.g. 'help action', 'help import').";

    public ValueTask ExecuteAsync(CommandContext context, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (arguments.Count == 1)
        {
            var name = arguments[0];
            if (_registry.TryGetCommand(name, out var command))
            {
                context.WriteInfo($"Help: {command.Name}");
                if (!string.IsNullOrWhiteSpace(command.Description))
                {
                    context.WriteInfo(command.Description);
                }
                if (!string.IsNullOrWhiteSpace(command.Usage))
                {
                    context.WriteInfo("Usage: " + command.Usage);
                }
                if (!string.IsNullOrWhiteSpace(command.HelpText))
                {
                    foreach (var line in command.HelpText.Split(Environment.NewLine))
                    {
                        context.WriteInfo(line);
                    }
                }
                return ValueTask.CompletedTask;
            }

            context.WriteWarning($"Unknown command '{name}'.");
            return ValueTask.CompletedTask;
        }

        var builder = new StringBuilder();
        builder.AppendLine("Available commands (use 'help <command>' for details):");

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
