using DroneFleet.Domain.Services;
using DroneFleet.Infrastructure.Logging;

namespace DroneFleet.App.ConsoleApp;

/// <summary>
/// Provides the interactive console shell for managing the drone fleet.
/// </summary>
internal sealed class FleetConsoleApp(CommandRegistry registry, IDroneFleetService fleetService, IAppLogger? logger, TextReader? input = null, TextWriter? output = null)
{
    private readonly CommandRegistry _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    private readonly IDroneFleetService _fleetService = fleetService ?? throw new ArgumentNullException(nameof(fleetService));
    private readonly TextReader _input = input ?? Console.In;
    private readonly TextWriter _output = output ?? Console.Out;
    private readonly IAppLogger? _logger = logger;

    /// <summary>
    /// Runs the command loop until the user exits or cancellation is requested.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var shutdownCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        void HandleCancel(object? sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            if (!shutdownCts.IsCancellationRequested)
            {
                shutdownCts.Cancel();
                _output.WriteLine();
                _output.WriteWarningLine("Cancellation requested. Press Enter to continue or type 'exit' to quit.");
            }
        }

        Console.CancelKeyPress += HandleCancel;

        try
        {
            while (!shutdownCts.IsCancellationRequested)
            {
                _output.Write(">>> ");
                var line = await _input.ReadLineAsync();
                if (line is null)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var tokens = CommandTokenizer.Tokenize(line);
                if (tokens.Count == 0)
                {
                    continue;
                }

                var commandName = tokens[0];
                var arguments = tokens.Skip(1).ToArray();

                if (!_registry.TryGetCommand(commandName, out var command))
                {
                    _output.WriteWarningLine($"Unknown command '{commandName}'. Type 'help' to see available commands.");
                    continue;
                }

                using var commandCts = CancellationTokenSource.CreateLinkedTokenSource(shutdownCts.Token);
                var context = new CommandContext(_fleetService, _input, _output, shutdownCts.Cancel, shutdownCts.Token, _logger);

                try
                {
                    await command.ExecuteAsync(context, arguments, commandCts.Token);
                }
                catch (OperationCanceledException)
                {
                    if (!shutdownCts.IsCancellationRequested)
                    {
                        _output.WriteWarningLine("Operation cancelled.");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error("Command execution failure", ex);
                    _output.WriteErrorLine($"Error: {ex.Message}");
                }
            }
        }
        finally
        {
            Console.CancelKeyPress -= HandleCancel;
        }
    }
}
