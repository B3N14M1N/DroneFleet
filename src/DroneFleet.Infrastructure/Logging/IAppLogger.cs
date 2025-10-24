namespace DroneFleet.Infrastructure.Logging;

/// <summary>
/// Simple application-wide logging abstraction.
/// </summary>
public interface IAppLogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? exception = null);
}
