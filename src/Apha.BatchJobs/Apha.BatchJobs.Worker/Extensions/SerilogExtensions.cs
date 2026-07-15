using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Microsoft.Extensions.Configuration;

namespace Apha.BatchJobs.Worker.Extensions;

/// <summary>
/// Serilog configuration extensions for local and AWS CloudWatch logging.
/// CloudWatch is optional and configured via environment during deployment.
/// Foundation layer provides local console/file logging only.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Adds structured console logging for non-local environments.
    /// CloudWatch sink can be added during deployment via environment-specific configuration.
    /// </summary>
    /// <param name="loggerConfiguration">The Serilog logger configuration.</param>
    /// <param name="configuration">Application configuration for future CloudWatch settings.</param>
    /// <returns>Configured LoggerConfiguration ready for CreateLogger().</returns>
    public static LoggerConfiguration UseStructuredConsoleLogging(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration? configuration = null)
    {
        var logStreamPrefix = ResolveLogStreamPrefix(configuration);

        // Enrich logs with context information
        loggerConfiguration = loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "FPSBatchJobs")
            .Enrich.WithProperty("Environment", GetEnvironment())
            .Enrich.WithProperty("LogStreamPrefix", logStreamPrefix);

        // Use readable console format for Demo/Development, JSON for Production
        var environment = GetEnvironment();
        if (environment.Equals("Demo", StringComparison.OrdinalIgnoreCase) ||
            environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            // Human-readable format for local testing
            loggerConfiguration = loggerConfiguration
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{LogStreamPrefix}] {Message:lj} {Properties:j}{NewLine}{Exception}");
        }
        else
        {
            // Structured JSON format for production/CloudWatch
            loggerConfiguration = loggerConfiguration
                .WriteTo.Console(new RenderedCompactJsonFormatter());
        }

        // Note: AWS CloudWatch logging is configured at deployment time via ECS task definition
        // and environment-specific appsettings configuration. Foundation layer uses console logging.
        // This allows CloudWatch to be added as an optional sink without requiring NuGet packages
        // that have security vulnerabilities during development.

        return loggerConfiguration;
    }

    /// <summary>
    /// Determines if running in local development environment.
    /// </summary>
    private static bool IsLocalEnvironment()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        return env.Equals("local", StringComparison.OrdinalIgnoreCase)
            || env.Equals("development", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the current environment name.
    /// </summary>
    private static string GetEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    }

    /// <summary>
    /// Resolves a stable prefix that appears on each log event.
    /// Exposed internally so <see cref="ProgramExtension.ConfigureLogging"/> can use the same logic.
    /// Precedence: env var BATCH_LOG_STREAM_PREFIX, config Logging:LogStreamPrefix, default value.
    /// </summary>
    internal static string ResolveLogStreamPrefix(IConfiguration? configuration)
    {
        var envPrefix = Environment.GetEnvironmentVariable("BATCH_LOG_STREAM_PREFIX");
        if (!string.IsNullOrWhiteSpace(envPrefix))
        {
            return envPrefix.Trim();
        }

        var configPrefix = configuration?["Logging:LogStreamPrefix"];
        if (!string.IsNullOrWhiteSpace(configPrefix))
        {
            return configPrefix.Trim();
        }

        return "apha-batch";
    }
}
