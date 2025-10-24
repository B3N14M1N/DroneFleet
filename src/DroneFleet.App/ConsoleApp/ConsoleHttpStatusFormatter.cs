using DroneFleet.Domain.Common;

namespace DroneFleet.App.ConsoleApp;

/// <summary>
/// Maps internal result codes to simple HTTP-like status codes and phrases for console display.
/// </summary>
internal static class ConsoleHttpStatusFormatter
{
    public static (int code, string phrase) Map(Result result)
    {
        if (result.IsSuccess)
        {
            return (200, "OK");
        }

        return Map(result.ErrorCode);
    }

    public static (int code, string phrase) Map(string? errorCode)
    {
        return errorCode switch
        {
            ResultCodes.Validation => (400, "Bad Request"),
            ResultCodes.NotFound => (404, "Not Found"),
            ResultCodes.DuplicateKey => (409, "Conflict"),
            _ => (500, "Error")
        };
    }

    public static string Format(Result result, string? messageOverride = null)
    {
        var (code, phrase) = Map(result);
        var message = messageOverride ?? (result.IsSuccess ? "Success" : result.Error ?? phrase);
        return $"{code} {phrase} - {message}";
    }

    public static string Format(ResultCodesWrapper wrapper)
    {
        var (code, phrase) = Map(wrapper.Code);
        return $"{code} {phrase}";
    }
}

/// <summary>
/// Simple wrapper for exposing raw error code mapping when only the code string is available.
/// </summary>
internal readonly struct ResultCodesWrapper(string? code)
{
    public string? Code { get; } = code;
}
