using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;

namespace DigitalStokvel.Common.Logging;

/// <summary>
/// Centralized logging configuration for Digital Stokvel platform
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configure Serilog with standard settings for Digital Stokvel services
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <param name="serviceName">Name of the service (e.g., "GroupService", "ContributionService")</param>
    /// <param name="environment">Environment name (Development, Staging, Production)</param>
    /// <returns>Configured LoggerConfiguration</returns>
    public static LoggerConfiguration CreateDefaultConfiguration(
        IConfiguration? configuration = null,
        string? serviceName = null,
        string? environment = null)
    {
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

        // Enrich with standard properties
        loggerConfig
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("MachineName", Environment.MachineName);

        // Add service name if provided
        if (!string.IsNullOrEmpty(serviceName))
        {
            loggerConfig.Enrich.WithProperty("ServiceName", serviceName);
        }

        // Add environment if provided
        if (!string.IsNullOrEmpty(environment))
        {
            loggerConfig.Enrich.WithProperty("Environment", environment);
        }

        // Add application version
        var version = typeof(LoggingConfiguration).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        loggerConfig.Enrich.WithProperty("Version", version);

        // Configure from appsettings if available
        if (configuration != null)
        {
            loggerConfig.ReadFrom.Configuration(configuration);
        }

        return loggerConfig;
    }

    /// <summary>
    /// Add console sink with structured output
    /// </summary>
    public static LoggerConfiguration AddConsoleSink(
        this LoggerConfiguration loggerConfig,
        LogEventLevel minimumLevel = LogEventLevel.Information)
    {
        return loggerConfig.WriteTo.Console(
            restrictedToMinimumLevel: minimumLevel,
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
    }

    /// <summary>
    /// Add file sink with daily rolling files
    /// </summary>
    public static LoggerConfiguration AddFileSink(
        this LoggerConfiguration loggerConfig,
        string logPath = "logs/digitalstokvel-.log",
        LogEventLevel minimumLevel = LogEventLevel.Information,
        long fileSizeLimitBytes = 100_000_000, // 100MB
        int retainedFileCountLimit = 30)
    {
        return loggerConfig.WriteTo.Async(a => a.File(
            path: logPath,
            rollingInterval: RollingInterval.Day,
            restrictedToMinimumLevel: minimumLevel,
            fileSizeLimitBytes: fileSizeLimitBytes,
            retainedFileCountLimit: retainedFileCountLimit,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"));
    }

    /// <summary>
    /// Add Seq sink for centralized logging (optional, requires Seq server)
    /// </summary>
    public static LoggerConfiguration AddSeqSink(
        this LoggerConfiguration loggerConfig,
        string seqServerUrl,
        string? apiKey = null,
        LogEventLevel minimumLevel = LogEventLevel.Debug)
    {
        if (string.IsNullOrEmpty(seqServerUrl))
        {
            return loggerConfig;
        }

        return loggerConfig.WriteTo.Async(a => a.Seq(
            serverUrl: seqServerUrl,
            apiKey: apiKey,
            restrictedToMinimumLevel: minimumLevel));
    }

    /// <summary>
    /// Configure logging for development environment
    /// </summary>
    public static LoggerConfiguration ConfigureForDevelopment(
        string serviceName,
        IConfiguration? configuration = null)
    {
        return CreateDefaultConfiguration(configuration, serviceName, "Development")
            .MinimumLevel.Debug()
            .AddConsoleSink(LogEventLevel.Debug)
            .AddFileSink("logs/dev/digitalstokvel-.log", LogEventLevel.Debug);
    }

    /// <summary>
    /// Configure logging for production environment
    /// </summary>
    public static LoggerConfiguration ConfigureForProduction(
        string serviceName,
        IConfiguration configuration,
        string? seqServerUrl = null,
        string? seqApiKey = null)
    {
        var config = CreateDefaultConfiguration(configuration, serviceName, "Production")
            .MinimumLevel.Information()
            .AddConsoleSink(LogEventLevel.Information)
            .AddFileSink("logs/digitalstokvel-.log", LogEventLevel.Warning);

        // Add Seq if configured
        if (!string.IsNullOrEmpty(seqServerUrl))
        {
            config.AddSeqSink(seqServerUrl, seqApiKey, LogEventLevel.Information);
        }

        return config;
    }
}
