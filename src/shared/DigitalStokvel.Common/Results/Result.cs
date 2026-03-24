namespace DigitalStokvel.Common.Results;

/// <summary>
/// Represents the result of an operation with success/failure state
/// </summary>
/// <typeparam name="T">The type of the result value</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }

    private Result(bool isSuccess, T? value, string? error, string? errorCode, Dictionary<string, string[]>? validationErrors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Create a successful result
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null, null, null);

    /// <summary>
    /// Create a failed result
    /// </summary>
    public static Result<T> Failure(string error, string? errorCode = null)
        => new(false, default, error, errorCode, null);

    /// <summary>
    /// Create a failed result with validation errors
    /// </summary>
    public static Result<T> ValidationFailure(Dictionary<string, string[]> validationErrors)
        => new(false, default, "Validation failed", "VALIDATION_ERROR", validationErrors);

    /// <summary>
    /// Create a failed result with a single validation error
    /// </summary>
    public static Result<T> ValidationFailure(string propertyName, string errorMessage)
        => new(false, default, errorMessage, "VALIDATION_ERROR", new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        });
}

/// <summary>
/// Represents the result of an operation without a return value
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }
    public Dictionary<string, string[]>? ValidationErrors { get; }

    private Result(bool isSuccess, string? error, string? errorCode, Dictionary<string, string[]>? validationErrors)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Create a successful result
    /// </summary>
    public static Result Success() => new(true, null, null, null);

    /// <summary>
    /// Create a failed result
    /// </summary>
    public static Result Failure(string error, string? errorCode = null)
        => new(false, error, errorCode, null);

    /// <summary>
    /// Create a failed result with validation errors
    /// </summary>
    public static Result ValidationFailure(Dictionary<string, string[]> validationErrors)
        => new(false, "Validation failed", "VALIDATION_ERROR", validationErrors);

    /// <summary>
    /// Create a failed result with a single validation error
    /// </summary>
    public static Result ValidationFailure(string propertyName, string errorMessage)
        => new(false, errorMessage, "VALIDATION_ERROR", new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        });
}
