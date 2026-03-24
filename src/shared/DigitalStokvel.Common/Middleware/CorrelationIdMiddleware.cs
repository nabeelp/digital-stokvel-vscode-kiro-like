using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace DigitalStokvel.Common.Middleware;

/// <summary>
/// Middleware to add correlation ID to all requests for distributed tracing
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or generate correlation ID
        var correlationId = GetOrGenerateCorrelationId(context);

        // Add to response headers
        context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);

        // Add to Serilog log context
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Request started with correlation ID: {CorrelationId}", correlationId);

            await _next(context);

            _logger.LogDebug("Request completed with correlation ID: {CorrelationId}", correlationId);
        }
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Try to get from request headers
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) &&
            !string.IsNullOrEmpty(correlationId))
        {
            return correlationId.ToString();
        }

        // Generate new correlation ID
        return Guid.NewGuid().ToString();
    }
}
