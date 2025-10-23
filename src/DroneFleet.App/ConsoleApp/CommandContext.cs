using DroneFleet.Domain.Services;

namespace DroneFleet.App.ConsoleApp;

/// <summary>
/// Provides shared services and helpers for commands.
/// </summary>
internal sealed class CommandContext(IDroneFleetService fleetService, TextReader input, TextWriter output, Action requestExit, CancellationToken shutdownToken)
{
    private readonly Action _requestExit = requestExit ?? throw new ArgumentNullException(nameof(requestExit));

    /// <summary>
    /// Gets the shared fleet service.
    /// </summary>
    public IDroneFleetService FleetService { get; } = fleetService ?? throw new ArgumentNullException(nameof(fleetService));

    /// <summary>
    /// Gets the input stream used for reading user input.
    /// </summary>
    public TextReader Input { get; } = input ?? throw new ArgumentNullException(nameof(input));

    /// <summary>
    /// Gets the output stream used for writing messages.
    /// </summary>
    public TextWriter Output { get; } = output ?? throw new ArgumentNullException(nameof(output));

    /// <summary>
    /// Gets a token that is cancelled when the application is shutting down.
    /// </summary>
    public CancellationToken ShutdownToken { get; } = shutdownToken;

    /// <summary>
    /// Requests the application to exit the command loop.
    /// </summary>
    public void RequestExit() => _requestExit();

    public void WriteError(string message) => Output.WriteErrorLine(message);

    public void WriteWarning(string message) => Output.WriteWarningLine(message);

    public void WriteSuccess(string message) => Output.WriteSuccessLine(message);

    public void WriteInfo(string message) => Output.WriteInfoLine(message);
}
