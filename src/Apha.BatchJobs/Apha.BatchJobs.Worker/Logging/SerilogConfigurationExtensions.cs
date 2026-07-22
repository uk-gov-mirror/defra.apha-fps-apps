using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

namespace Apha.BatchJobs.Worker.Logging;

/// <summary>
/// Serilog creation, enrichment and provider registration for the batch worker host.
/// Sink/formatter selection is hardcoded here per environment — matching the pattern already
/// used by the sibling Apha.PACT/Apha.PIMS/Apha.FPS APIs (see their own SerilogExtensions /
/// Program.cs) — rather than expressed as a <c>Serilog:WriteTo</c> array in appsettings.json.
/// <c>ReadFrom.Configuration</c> is still used, but only for <c>MinimumLevel</c>/overrides.
/// </summary>
public static class SerilogConfigurationExtensions
{
    /// <summary>
    /// Creates the Serilog logger and wires it into the host's logging pipeline. Must run after
    /// <c>ConfigureWorkerConfiguration</c> so the legacy <c>BATCH_LOG_STREAM_PREFIX</c> compat
    /// mapping has already resolved into <c>Logging:LogStreamPrefix</c> — this method reads that
    /// key directly rather than re-checking the legacy environment variable itself.
    /// </summary>
    public static void ConfigureWorkerLogging(this HostApplicationBuilder builder)
    {
        // SelfLog is diagnostic noise in production; only worth the console clutter locally or
        // when explicitly opted into for troubleshooting a specific deployed environment.
        var selfLogEnabled = builder.Environment.IsDevelopment()
            || builder.Configuration.GetValue<bool>("Serilog:SelfLogEnabled");

        if (selfLogEnabled)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);
        }

        var configuredLogStreamPrefix = builder.Configuration["Logging:LogStreamPrefix"] ?? "apha-batch";

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "FPSBatchJobs")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            // Named "Configured..." because this is metadata describing intent, not the literal
            // CloudWatch log stream identifier ECS assigns at runtime.
            .Enrich.WithProperty("ConfiguredLogStreamPrefix", configuredLogStreamPrefix);

        if (builder.Environment.IsEnvironment("local"))
        {
            var logPath = builder.Configuration.GetValue<string>("LogsPath") is { Length: > 0 } configuredPath
                ? Path.Combine(configuredPath, "Logsample.log")
                : Path.Combine("Logs", "Logsample.log");

            loggerConfiguration
                .WriteTo.Console()
                .WriteTo.File(logPath, Serilog.Events.LogEventLevel.Verbose, rollingInterval: RollingInterval.Day);
        }
        else
        {
            loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
    }
}
