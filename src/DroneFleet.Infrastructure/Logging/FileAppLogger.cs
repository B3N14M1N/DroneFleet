using System.Collections.Concurrent;
using System.Text;

namespace DroneFleet.Infrastructure.Logging;

/// <summary>
/// File-based logger writing timestamped entries; creates directory if needed.
/// </summary>
public sealed class FileAppLogger : IAppLogger, IDisposable
{
    private readonly string _filePath;
    private readonly BlockingCollection<string> _queue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _background;

    public FileAppLogger(string filePath)
    {
        _filePath = string.IsNullOrWhiteSpace(filePath)
            ? throw new ArgumentException("File path cannot be empty", nameof(filePath))
            : Path.GetFullPath(filePath);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _background = Task.Run(ConsumeAsync);
    }

    public void Info(string message) => Enqueue("INFO", message);
    public void Warn(string message) => Enqueue("WARN", message);
    public void Error(string message, Exception? exception = null)
    {
        var builder = new StringBuilder(message);
        if (exception is not null)
        {
            builder.Append(" | ").Append(exception.GetType().Name).Append(": ").Append(exception.Message);
        }
        Enqueue("ERROR", builder.ToString());
    }

    private void Enqueue(string level, string message)
    {
        var line = $"{DateTimeOffset.UtcNow:O} [{level}] {message}";
        _queue.Add(line);
    }

    private async Task ConsumeAsync()
    {
        try
        {
            using var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            foreach (var line in _queue.GetConsumingEnumerable(_cts.Token))
            {
                await writer.WriteLineAsync(line);
                await writer.FlushAsync();
            }
        }
        catch
        {
            // Swallow logging exceptions to avoid crashing the app.
        }
    }

    public void Dispose()
    {
        _queue.CompleteAdding();
        _cts.Cancel();
        try { _background.Wait(2000); } catch { }
        _cts.Dispose();
    }
}
