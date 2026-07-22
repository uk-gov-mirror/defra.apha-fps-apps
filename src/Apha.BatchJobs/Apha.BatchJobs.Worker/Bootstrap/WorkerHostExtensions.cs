using Apha.BatchJobs.Application.DependencyInjection;
using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Worker.Configuration;
using Apha.BatchJobs.Worker.Execution;
using Apha.BatchJobs.Worker.Reporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Apha.BatchJobs.Worker.Bootstrap;

/// <summary>
/// Configuration layering, typed option registration, and service registration for the batch
/// worker host. Serilog setup lives in <c>Logging/SerilogConfigurationExtensions.cs</c> instead —
/// kept separate so logging can be wired up before <c>host.Build()</c> independently of DI.
/// </summary>
public static class WorkerHostExtensions
{
    /// <summary>
    /// Legacy single-underscore ECS environment variable names and the standard config keys
    /// they map to. Used only as a fallback when the standard key has no value from any other
    /// source (appsettings.json, appsettings.{Environment}.json, or the standard
    /// <c>BatchJobs__GracefulShutdownWindowSeconds</c>-style double-underscore env var, which
    /// the default <see cref="Host.CreateApplicationBuilder(string[])"/> configuration already
    /// recognizes without any mapping).
    /// </summary>
    private static readonly (string LegacyEnvironmentVariable, string ConfigurationKey)[] LegacyEnvironmentVariableMap =
    [
        ("BATCH_JOB_TIMEOUT_SECONDS", "BatchJobs:WorkerOverallTimeoutSeconds"),
        ("BATCH_GRACEFUL_SHUTDOWN_WINDOW_SECONDS", "BatchJobs:GracefulShutdownWindowSeconds"),
        ("BATCH_DB_COMMAND_TIMEOUT_SECONDS", "BatchJobs:DbCommandTimeoutSeconds"),
        ("BATCH_LOCK_TIMEOUT_SECONDS", "BatchJobs:LockTimeoutSeconds"),
        ("BATCH_LOG_STREAM_PREFIX", "Logging:LogStreamPrefix"),
    ];

    /// <summary>
    /// Layers <c>appsettings.Local.json</c> in over what <see cref="Host.CreateApplicationBuilder(string[])"/>
    /// already loaded (appsettings.json, appsettings.{Environment}.json, environment variables),
    /// re-asserts environment variables so they retain top precedence, then fills in any legacy
    /// single-underscore ECS variable only where the standard config key is still unset.
    /// </summary>
    public static void ConfigureWorkerConfiguration(this HostApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();

        ApplyLegacyEnvironmentVariableCompat(builder.Configuration);
    }

    private static void ApplyLegacyEnvironmentVariableCompat(ConfigurationManager configuration)
    {
        var fallbacks = new Dictionary<string, string?>();

        foreach (var (legacyEnvironmentVariable, configurationKey) in LegacyEnvironmentVariableMap)
        {
            if (!string.IsNullOrWhiteSpace(configuration[configurationKey]))
                continue;

            var legacyValue = Environment.GetEnvironmentVariable(legacyEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(legacyValue))
                fallbacks[configurationKey] = legacyValue;
        }

        if (fallbacks.Count > 0)
            configuration.AddInMemoryCollection(fallbacks);
    }

    /// <summary>
    /// Registers worker dependencies: the existing batch job services (unchanged), the new
    /// validated <see cref="BatchRuntimeOptions"/>, and the shared <see cref="BatchFailureClassifier"/>.
    /// </summary>
    public static void ConfigureWorkerServices(this HostApplicationBuilder builder)
    {
        ServiceCollectionSetup.ConfigureBatchJobServices(builder.Services, builder.Configuration);

        builder.Services
            .AddOptions<BatchRuntimeOptions>()
            .Bind(builder.Configuration.GetSection(BatchRuntimeOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<BatchFailureClassifier>();
        builder.Services.AddSingleton<IBatchRunSummaryWriter, BatchRunSummaryWriter>();
        builder.Services.AddSingleton<BatchExecutionRequestResolver>();
        builder.Services.AddSingleton<IBatchWorkerRunner, BatchWorkerRunner>();
    }

    /// <summary>
    /// Stops the host bounded by <see cref="BatchRuntimeOptions.GracefulShutdownWindowSeconds"/>,
    /// leaving a safety margin below the ECS SIGTERM→forced-stop deadline. Reads configuration
    /// directly (not through DI) so it still works if the host is in a partially-failed state.
    /// Shutdown failure is logged but never replaces the primary batch exit code — the caller's
    /// `finally` block calls this after the exit code has already been decided.
    /// </summary>
    public static async Task StopSafelyAsync(this IHost host, IConfiguration configuration)
    {
        var gracefulShutdownWindowSeconds = configuration.GetValue<int?>(
            $"{BatchRuntimeOptions.SectionName}:GracefulShutdownWindowSeconds");

        if (gracefulShutdownWindowSeconds is not > 0)
            gracefulShutdownWindowSeconds = 25;

        using var stopCts = new CancellationTokenSource(TimeSpan.FromSeconds(gracefulShutdownWindowSeconds.Value));

        try
        {
            await host.StopAsync(stopCts.Token);
            Log.Information(
                "Host stopped | GracefulShutdownWindowSeconds={GracefulShutdownWindowSeconds}",
                gracefulShutdownWindowSeconds);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Host stop encountered an issue during shutdown");
        }
    }
}
