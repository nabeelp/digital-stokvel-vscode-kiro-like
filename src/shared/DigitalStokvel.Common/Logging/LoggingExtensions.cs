using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace DigitalStokvel.Common.Logging;

/// <summary>
/// Logging extensions for structured logging and correlation tracking
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Log with correlation ID for distributed tracing
    /// </summary>
    public static IDisposable BeginCorrelationScope(this ILogger logger, string correlationId)
    {
        return LogContext.PushProperty("CorrelationId", correlationId);
    }

    /// <summary>
    /// Log with user context
    /// </summary>
    public static IDisposable BeginUserScope(this ILogger logger, Guid userId, string? userName = null)
    {
        var disposables = new List<IDisposable>
        {
            LogContext.PushProperty("UserId", userId)
        };

        if (!string.IsNullOrEmpty(userName))
        {
            disposables.Add(LogContext.PushProperty("UserName", userName));
        }

        return new CompositeDisposable(disposables);
    }

    /// <summary>
    /// Log with group context for stokvel operations
    /// </summary>
    public static IDisposable BeginGroupScope(this ILogger logger, Guid groupId, string? groupName = null)
    {
        var disposables = new List<IDisposable>
        {
            LogContext.PushProperty("GroupId", groupId)
        };

        if (!string.IsNullOrEmpty(groupName))
        {
            disposables.Add(LogContext.PushProperty("GroupName", groupName));
        }

        return new CompositeDisposable(disposables);
    }

    /// <summary>
    /// Log with transaction context
    /// </summary>
    public static IDisposable BeginTransactionScope(
        this ILogger logger,
        string transactionType,
        Guid transactionId,
        decimal? amount = null)
    {
        var disposables = new List<IDisposable>
        {
            LogContext.PushProperty("TransactionType", transactionType),
            LogContext.PushProperty("TransactionId", transactionId)
        };

        if (amount.HasValue)
        {
            disposables.Add(LogContext.PushProperty("Amount", amount.Value));
        }

        return new CompositeDisposable(disposables);
    }

    /// <summary>
    /// Log sensitive data with masking
    /// </summary>
    public static void LogSensitive(
        this ILogger logger,
        LogLevel logLevel,
        string message,
        params (string Key, string Value)[] sensitiveData)
    {
        var maskedData = sensitiveData
            .Select(d => (d.Key, Value: MaskSensitiveData(d.Value)))
            .ToArray();

        var messageWithData = message + " " +
            string.Join(", ", maskedData.Select(d => $"{d.Key}={d.Value}"));

        logger.Log(logLevel, messageWithData);
    }

    /// <summary>
    /// Mask sensitive data (show last 4 characters)
    /// </summary>
    private static string MaskSensitiveData(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= 4)
            return "****";

        return new string('*', value.Length - 4) + value[^4..];
    }

    /// <summary>
    /// Log performance metrics
    /// </summary>
    public static IDisposable BeginPerformanceScope(
        this ILogger logger,
        string operationName,
        string? additionalInfo = null)
    {
        var startTime = DateTimeOffset.UtcNow;
        logger.LogDebug("Starting operation: {OperationName} {AdditionalInfo}",
            operationName, additionalInfo ?? string.Empty);

        return new PerformanceScope(logger, operationName, startTime);
    }

    /// <summary>
    /// Composite disposable for multiple log contexts
    /// </summary>
    private class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables;

        public CompositeDisposable(List<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Performance tracking scope
    /// </summary>
    private class PerformanceScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly DateTimeOffset _startTime;

        public PerformanceScope(ILogger logger, string operationName, DateTimeOffset startTime)
        {
            _logger = logger;
            _operationName = operationName;
            _startTime = startTime;
        }

        public void Dispose()
        {
            var duration = DateTimeOffset.UtcNow - _startTime;
            _logger.LogInformation(
                "Completed operation: {OperationName} in {Duration}ms",
                _operationName,
                duration.TotalMilliseconds);
        }
    }
}
