using DroneFleet.App.ConsoleApp.Commands;

namespace DroneFleet.App.ConsoleApp;

/// <summary>
/// Maintains a lookup of registered console commands.
/// </summary>
internal sealed class CommandRegistry
{
    private readonly Dictionary<string, IConsoleCommand> _commands = new(StringComparer.OrdinalIgnoreCase);

    public void Register(IConsoleCommand command, params string[] aliases)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_commands.TryAdd(command.Name, command))
        {
            throw new InvalidOperationException($"Command '{command.Name}' is already registered.");
        }

        if (aliases is null)
        {
            return;
        }

        foreach (var alias in aliases)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                continue;
            }

            if (!_commands.TryAdd(alias, command))
            {
                throw new InvalidOperationException($"Alias '{alias}' is already registered.");
            }
        }
    }

    public bool TryGetCommand(string name, out IConsoleCommand command)
    {
        return _commands.TryGetValue(name, out command!);
    }

    public IReadOnlyCollection<IConsoleCommand> Commands => _commands.Values.Distinct().ToArray();
}
