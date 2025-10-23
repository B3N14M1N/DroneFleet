using System.Diagnostics.CodeAnalysis;

namespace DroneFleet.Domain.Common;

/// <summary>
/// Represents the outcome of an operation without a return value.
/// </summary>
public readonly struct Result
{
    private Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message when the operation fails.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets the optional error code that categorises the failure.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, null, null);

    /// <summary>
    /// Creates a failure result with the provided error message and optional code.
    /// </summary>
    public static Result Failure(string error, string? errorCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new Result(false, error, errorCode);
    }

    /// <summary>
    /// Creates a failure result based on another one.
    /// </summary>
    public static Result From(Result result) =>
        result.IsSuccess ? Success() : Failure(result.Error ?? string.Empty, result.ErrorCode);
}

/// <summary>
/// Represents the outcome of an operation returning a value.
/// </summary>
/// <typeparam name="T">Type of the returned value.</typeparam>
public readonly struct Result<T>
{
    private Result(bool isSuccess, T? value, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the value returned by the operation when successful.
    /// </summary>
    [MaybeNull]
    public T? Value { get; }

    /// <summary>
    /// Gets the error message when the operation fails.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets the optional error code that categorises the failure.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null, null);

    /// <summary>
    /// Creates a failure result with the provided error message and optional code.
    /// </summary>
    public static Result<T> Failure(string error, string? errorCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new Result<T>(false, default, error, errorCode);
    }
}

/// <summary>
/// Provides well-known error codes for results.
/// </summary>
public static class ResultCodes
{
    /// <summary>
    /// Indicates that an entity with the requested key already exists.
    /// </summary>
    public const string DuplicateKey = "duplicate";

    /// <summary>
    /// Indicates that a requested entity could not be found.
    /// </summary>
    public const string NotFound = "not_found";

    /// <summary>
    /// Indicates a validation failure.
    /// </summary>
    public const string Validation = "validation";
}
