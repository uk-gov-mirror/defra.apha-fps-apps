using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Application.DependencyInjection;
using Apha.BatchJobs.Worker.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;
using System.Globalization;

var builder = Host.CreateApplicationBuilder(args);

var isLocalOrDevelopment = builder.Environment.IsDevelopment()
    || builder.Environment.IsEnvironment("Local")
    || builder.Environment.IsEnvironment("local");
var strictExecutionContractMode = ResolveStrictExecutionContractMode(builder.Configuration, isLocalOrDevelopment);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
var startedAt = DateTime.UtcNow;
const int ExitCodeSuccess = 0;
const int ExitCodeBusinessFailure = 1;
const int ExitCodeConfigurationError = 2;
const int ExitCodeCancelled = 3;
const int ExitCodeDependencyOutage = 5;

// ECS SIGTERM â†’ forced-stop window is typically 30 s.
// We allow 25 s for graceful cleanup before the host forces termination.
var gracefulShutdownWindowSeconds = ResolveIntSetting(
    builder.Configuration,
    "BatchJobs:GracefulShutdownWindowSeconds",
    "BATCH_GRACEFUL_SHUTDOWN_WINDOW_SECONDS",
    25);

if (builder.Environment.IsEnvironment("local"))
{
    string srvpath = builder.Configuration.GetValue<string>("LogsPath") ?? string.Empty;
    string logpath = $"{(builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("local") ? "Logs" : srvpath)}\\Logsample.log";
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File(logpath, Serilog.Events.LogEventLevel.Verbose, rollingInterval: RollingInterval.Day)
        .CreateLogger();
}
else
{
    Serilog.Debugging.SelfLog.Enable(Console.Error);
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .UseStructuredConsoleLogging()
        .CreateLogger();
}

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);
builder.ConfigureServices();

using var host = builder.Build();
var serviceProvider = host.Services;

ILoggerFactory? loggerFactory = null;
string failureCategory = "BusinessFailure";
string runOutcome = "Failed";
string? requestedJobName = null;
string requestedRunMode = "Manual";
string? capturedJobExecutionId = null;
string? capturedJobQueueId = null;
int? capturedExecutionId = null;
var exitCode = ExitCodeBusinessFailure;
bool gracefulShutdownCompleted = true; // Assume graceful unless proven otherwise

try
{
    await host.StartAsync();

    loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("BatchJobs.Startup");
    var hostLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
    
    logger.LogInformation("===========================================");
    logger.LogInformation("Batch Jobs Worker - Starting");
    logger.LogInformation("===========================================");
    logger.LogInformation("Timestamp: {StartTime:yyyy-MM-dd HH:mm:ss.fff}", startedAt);
    logger.LogInformation("ProcessId: {ProcessId}", Environment.ProcessId);
    logger.LogInformation("Environment: {EnvironmentName}", builder.Environment.EnvironmentName);

    await WaitForDebuggerAttachIfRequestedAsync(logger, cancellationToken: CancellationToken.None);

    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var dbCommandTimeoutSeconds = ResolveIntSetting(
        config,
        "BatchJobs:DbCommandTimeoutSeconds",
        "BATCH_DB_COMMAND_TIMEOUT_SECONDS",
        30);
    var lockTimeoutSeconds = ResolveIntSetting(
        config,
        "BatchJobs:LockTimeoutSeconds",
        "BATCH_LOCK_TIMEOUT_SECONDS",
        3600);
    var jobTimeoutSeconds = ResolveIntSetting(
        config,
        "BatchJobs:JobTimeout",
        "BATCH_JOB_TIMEOUT_SECONDS",
        3600);

    logger.LogInformation("Flow checkpoint: Program.Main -> Host.Started -> Resolving JobOrchestrator");
    logger.LogInformation(
        "Runtime tolerance | GracefulShutdownWindowSeconds={GracefulShutdownWindowSeconds} | DbCommandTimeoutSeconds={DbCommandTimeoutSeconds} | LockTimeoutSeconds={LockTimeoutSeconds} | JobTimeoutSeconds={JobTimeoutSeconds}",
        gracefulShutdownWindowSeconds,
        dbCommandTimeoutSeconds,
        lockTimeoutSeconds,
        jobTimeoutSeconds);

    // Get job name from args or environment variable
    var jobName = args.Length > 0 ? args[0] : (Environment.GetEnvironmentVariable("BATCH_JOB_NAME") ?? "HealthCheck");
    var runModeEnv = Environment.GetEnvironmentVariable("BATCH_RUN_MODE") ?? "Manual";
    var runMode = Enum.TryParse<RunMode>(runModeEnv, ignoreCase: true, out var parsedMode) ? parsedMode : RunMode.Manual;
    // External/API correlation key for tracing across trigger -> worker -> status.
    var jobExecutionIdEnv = Environment.GetEnvironmentVariable("BATCH_JOB_EXECUTION_ID")
        ?? Environment.GetEnvironmentVariable("BATCH_EXECUTION_ID");
    var requestedAtUtcEnv = Environment.GetEnvironmentVariable("BATCH_REQUESTED_AT_UTC");
    var requestedAtUtc = ParseRequestedAtUtc(requestedAtUtcEnv, logger);
    
    // For scheduled jobs, default to current UTC time if BATCH_REQUESTED_AT_UTC was not provided.
    // This captures the worker startup time and enables startup latency measurement.
    if (requestedAtUtc == null && runMode == RunMode.Scheduled)
    {
        requestedAtUtc = DateTime.UtcNow;
        logger.LogInformation(
            "BATCH_REQUESTED_AT_UTC not provided for scheduled job; defaulting to worker startup time | DefaultRequestedAtUtc={DefaultRequestedAtUtc}",
            requestedAtUtc);
    }
    
    var allowScheduledMabArchiveGeneratedExecutionId =
        runMode == RunMode.Scheduled
        && string.Equals(jobName, "MABArchive", StringComparison.OrdinalIgnoreCase);
    Guid jobExecutionId;
    string userId = "system";

    // Canonical requester identity env var for trigger -> worker contract.
    userId = Environment.GetEnvironmentVariable("BATCH_REQUESTED_BY")
        ?? "system";

    if (string.IsNullOrWhiteSpace(jobExecutionIdEnv))
    {
        if (strictExecutionContractMode && !allowScheduledMabArchiveGeneratedExecutionId)
        {
            throw new InvalidOperationException(
                "BATCH_JOB_EXECUTION_ID is required in strict execution contract mode.");
        }

        jobExecutionId = Guid.NewGuid();
        logger.LogWarning(
            "BATCH_JOB_EXECUTION_ID was missing. Generated fallback execution id | GeneratedJobExecutionId={GeneratedJobExecutionId}",
            jobExecutionId);
    }
    else if (!Guid.TryParse(jobExecutionIdEnv, out jobExecutionId))
    {
        if (strictExecutionContractMode && !allowScheduledMabArchiveGeneratedExecutionId)
        {
            throw new InvalidOperationException(
                "BATCH_JOB_EXECUTION_ID must be a valid GUID in strict execution contract mode.");
        }

        jobExecutionId = Guid.NewGuid();
        logger.LogWarning(
            "BATCH_JOB_EXECUTION_ID was invalid. Generated fallback execution id | ProvidedValue={ProvidedJobExecutionId} | GeneratedJobExecutionId={GeneratedJobExecutionId}",
            jobExecutionIdEnv,
            jobExecutionId);
    }

    var correlationService = serviceProvider.GetRequiredService<ICorrelationService>();
    correlationService.SetCorrelationId(jobExecutionId.ToString("D"));
    capturedJobExecutionId = jobExecutionId.ToString("D");

    logger.LogInformation(
        "Correlation context initialized | CorrelationId={CorrelationId} | Source={CorrelationSource}",
        jobExecutionId,
        !string.IsNullOrWhiteSpace(jobExecutionIdEnv) && Guid.TryParse(jobExecutionIdEnv, out _) ? "IncomingJobExecutionId" : "WorkerGeneratedFallback");

    requestedJobName = jobName;
    requestedRunMode = runMode.ToString();

    logger.LogInformation(
        "Requested job: {JobName} | RunMode: {RunMode} | JobExecutionId={JobExecutionId} | UserId={UserId} | RequestedAtUtc={RequestedAtUtc}",
        jobName,
        runMode,
        jobExecutionId,
        userId,
        requestedAtUtc?.ToString("O") ?? "n/a");

    // Cancel job execution only when the host is stopping.
    // Do not use GracefulShutdownWindowSeconds as a hard runtime cap.
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
        hostLifetime.ApplicationStopping);

    if (hostLifetime.ApplicationStopping.IsCancellationRequested)
    {
        logger.LogWarning(
            "Host stopping signal was already set before job start — skipping execution | JobName={JobName} | GracefulWindowSeconds={GracefulWindowSeconds}",
            jobName, gracefulShutdownWindowSeconds);
        exitCode = ExitCodeCancelled;
        failureCategory = "Cancellation";
        runOutcome = "Cancelled";
    }
    else
    {
        // Run through orchestrator (handles lock, execution records, and job execution)
        await using var executionScope = serviceProvider.CreateAsyncScope();
        var executionRepository = executionScope.ServiceProvider.GetRequiredService<IJobExecutionRepository>();

        await VerifyExecutionContractAsync(
            jobName,
            jobExecutionId,
            executionRepository,
            logger,
            linkedCts.Token);

        var orchestrator = executionScope.ServiceProvider.GetRequiredService<IJobOrchestrator>();
        var result = await orchestrator.RunAsync(jobName, runMode, jobExecutionId, userId, requestedAtUtc, linkedCts.Token);

        capturedJobQueueId = result.JobQueueId.ToString();
        capturedExecutionId = result.ExecutionId;

        logger.LogInformation("===========================================");
        logger.LogInformation(
            "Job '{JobName}' finished | Status={Status} | JobQueueId={JobQueueId} | ExecutionId={ExecutionId}",
            result.JobName, result.Status, result.JobQueueId, result.ExecutionId);
        logger.LogInformation("===========================================");

        exitCode = ExitCodeSuccess;
        failureCategory = "None";
        runOutcome = "Succeeded";
    }
}
catch (InvalidOperationException ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    var exceptionPrefix = GetExceptionTypePrefix(ex, builder.Configuration);
    logger?.LogError(ex, "{ExceptionType} Job factory error: {ErrorMessage}", exceptionPrefix, ex.Message);
    Console.Error.WriteLine($"ERROR: {exceptionPrefix} {ex.Message}");
    exitCode = ExitCodeConfigurationError;
    failureCategory = "ConfigurationError";
    runOutcome = "Failed";
}
catch (OperationCanceledException ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    var remainingWindowMs = Math.Max(0, (int)(gracefulShutdownWindowSeconds * 1000 - (DateTime.UtcNow - startedAt).TotalMilliseconds));
    logger?.LogWarning(ex,
        "Job was cancelled | JobName={JobName} | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId} | RemainingShutdownWindowMs={RemainingWindowMs}",
        requestedJobName ?? "Unknown", capturedJobQueueId ?? "N/A", capturedJobExecutionId ?? "N/A", remainingWindowMs);
    Console.Error.WriteLine($"CANCELLED: Job execution was cancelled");
    
    // Mark as forced cancellation if we ran out of graceful window (remainingWindowMs near 0)
    gracefulShutdownCompleted = remainingWindowMs > 100; // Allow 100ms buffer
    
    exitCode = ExitCodeCancelled;
    failureCategory = "Cancellation";
    runOutcome = "Cancelled";
}
catch (Exception ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    var exceptionPrefix = GetExceptionTypePrefix(ex, builder.Configuration);
    if (IsDependencyOutage(ex))
    {
        logger?.LogError(ex, "{ExceptionType} Dependency outage detected: {ErrorMessage}", exceptionPrefix, ex.Message);
        exitCode = ExitCodeDependencyOutage;
        failureCategory = "DependencyOutage";
    }
    else
    {
        logger?.LogError(ex, "{ExceptionType} Unhandled business/runtime exception: {ErrorMessage}", exceptionPrefix, ex.Message);
        exitCode = ExitCodeBusinessFailure;
        failureCategory = "BusinessFailure";
    }

    Console.Error.WriteLine($"FATAL ERROR: {ex}");
    runOutcome = "Failed";
}
finally
{
    try
    {
        var endedAtUtc = DateTime.UtcNow;
        var durationMs = (endedAtUtc - startedAt).TotalMilliseconds;
        var logLevel = failureCategory switch
        {
            "None" => LogLevel.Information,
            "Cancellation" => LogLevel.Warning,
            _ => LogLevel.Error
        };
        var humanReadableMessage = GenerateHumanReadableMessage(runOutcome, failureCategory);
        const string summaryTemplate = "Run completed | StartedAt={StartTime} | EndedAt={EndTime} | Outcome={Outcome} | FailureCategory={FailureCategory} | ExitCode={ExitCode} | Message={Message} | JobName={JobName} | JobQueueId={JobQueueId} | ExecutionId={ExecutionId} | JobExecutionId={JobExecutionId} | RunMode={RunMode} | TotalDurationMs={DurationMs} | GracefulShutdownCompleted={GracefulShutdownCompleted}";
        var summaryLine = BuildSummaryLine(
            startedAt,
            endedAtUtc,
            runOutcome,
            failureCategory,
            exitCode,
            humanReadableMessage,
            requestedJobName ?? "Unknown",
            capturedJobQueueId ?? "N/A",
            capturedExecutionId?.ToString() ?? "N/A",
            capturedJobExecutionId ?? "N/A",
            requestedRunMode,
            durationMs,
            gracefulShutdownCompleted);

        // Always emit one terminal summary to stdout for container-level visibility.
        Console.WriteLine(summaryLine);
        
        var logger = loggerFactory?.CreateLogger("BatchJobs.Summary");
        
        // Log at appropriate level by outcome semantics.
        if (logLevel == LogLevel.Information)
        {
            logger?.LogInformation(
                summaryTemplate,
                startedAt,
                endedAtUtc,
                runOutcome,
                failureCategory,
                exitCode,
                humanReadableMessage,
                requestedJobName ?? "Unknown",
                capturedJobQueueId ?? "N/A",
                capturedExecutionId?.ToString() ?? "N/A",
                capturedJobExecutionId ?? "N/A",
                requestedRunMode,
                durationMs,
                gracefulShutdownCompleted);
        }
        else if (logLevel == LogLevel.Warning)
        {
            logger?.LogWarning(
                summaryTemplate,
                startedAt,
                endedAtUtc,
                runOutcome,
                failureCategory,
                exitCode,
                humanReadableMessage,
                requestedJobName ?? "Unknown",
                capturedJobQueueId ?? "N/A",
                capturedExecutionId?.ToString() ?? "N/A",
                capturedJobExecutionId ?? "N/A",
                requestedRunMode,
                durationMs,
                gracefulShutdownCompleted);
        }
        else
        {
            logger?.LogError(
                summaryTemplate,
                startedAt,
                endedAtUtc,
                runOutcome,
                failureCategory,
                exitCode,
                humanReadableMessage,
                requestedJobName ?? "Unknown",
                capturedJobQueueId ?? "N/A",
                capturedExecutionId?.ToString() ?? "N/A",
                capturedJobExecutionId ?? "N/A",
                requestedRunMode,
                durationMs,
                gracefulShutdownCompleted);
        }
    }
    catch
    {
        // Preserve original exit behavior if summary logging itself fails.
    }

    try
    {
        await host.StopAsync();
    }
    catch (Exception ex)
    {
        var logger = loggerFactory?.CreateLogger("BatchJobs.Shutdown");
        logger?.LogWarning(ex, "Host stop encountered an issue during shutdown");
    }

    Log.CloseAndFlush();
}

static async Task WaitForDebuggerAttachIfRequestedAsync(Microsoft.Extensions.Logging.ILogger logger, CancellationToken cancellationToken)
{
    var waitForAttach = Environment.GetEnvironmentVariable("BATCH_DEBUG_WAIT_FOR_ATTACH");
    if (!string.Equals(waitForAttach, "true", StringComparison.OrdinalIgnoreCase) || System.Diagnostics.Debugger.IsAttached)
    {
        return;
    }

    var timeoutSecondsRaw = Environment.GetEnvironmentVariable("BATCH_DEBUG_ATTACH_TIMEOUT_SECONDS");
    var timeoutSeconds = int.TryParse(timeoutSecondsRaw, out var parsedTimeout) ? parsedTimeout : 120;
    var timeoutAt = DateTime.UtcNow.AddSeconds(Math.Max(0, timeoutSeconds));

    logger.LogInformation(
        "Waiting for debugger attach | ProcessId={ProcessId} | TimeoutSeconds={TimeoutSeconds}",
        Environment.ProcessId,
        timeoutSeconds);

    while (!System.Diagnostics.Debugger.IsAttached && DateTime.UtcNow < timeoutAt)
    {
        await Task.Delay(250, cancellationToken);
    }

    var breakOnStart = Environment.GetEnvironmentVariable("BATCH_DEBUG_BREAK_ON_START");
    if (System.Diagnostics.Debugger.IsAttached && string.Equals(breakOnStart, "true", StringComparison.OrdinalIgnoreCase))
    {
        logger.LogInformation("Debugger attached; breaking at worker startup | ProcessId={ProcessId}", Environment.ProcessId);
        System.Diagnostics.Debugger.Break();
    }

    logger.LogInformation(
        "Debugger attach wait finished | ProcessId={ProcessId} | DebuggerAttached={DebuggerAttached}",
        Environment.ProcessId,
        System.Diagnostics.Debugger.IsAttached);
}

return exitCode;

static string BuildSummaryLine(
    DateTime startedAt,
    DateTime endedAt,
    string outcome,
    string failureCategory,
    int exitCode,
    string message,
    string jobName,
    string jobQueueId,
    string executionId,
    string jobExecutionId,
    string runMode,
    double totalDurationMs,
    bool gracefulShutdownCompleted)
{
    return $"Run completed | StartedAt={startedAt:O} | EndedAt={endedAt:O} | Outcome={outcome} | FailureCategory={failureCategory} | ExitCode={exitCode} | Message={message} | JobName={jobName} | JobQueueId={jobQueueId} | ExecutionId={executionId} | JobExecutionId={jobExecutionId} | RunMode={runMode} | TotalDurationMs={totalDurationMs:F0} | GracefulShutdownCompleted={gracefulShutdownCompleted}";
}

static string GenerateHumanReadableMessage(string outcome, string failureCategory)
{
    return (outcome, failureCategory) switch
    {
        ("Succeeded", _) => "Job completed successfully within the graceful shutdown window.",
        ("Cancelled", "Cancellation") => "Job execution was cancelled due to host shutdown or timeout.",
        ("Failed", "ConfigurationError") => "Job failed due to configuration error (job not registered, invalid settings, etc.).",
        ("Failed", "BusinessFailure") => "Job failed with a business or runtime exception.",
        ("Failed", "DependencyOutage") => "Job failed due to dependency outage (database unavailable, network timeout, etc.).",
        _ => $"Job execution ended with outcome: {outcome} ({failureCategory})."
    };
}

static bool IsDependencyOutage(Exception ex)
{
    for (Exception? current = ex; current != null; current = current.InnerException)
    {
        if (current is NpgsqlException || current is TimeoutException || current is DbUpdateException)
            return true;
    }

    return false;
}

static string GetExceptionTypePrefix(Exception ex, IConfiguration? config = null)
{
    // Load exception type mappings from configuration
    var exceptionTypes = config?.GetSection("ExceptionTypes").Get<Dictionary<string, string>>()
        ?? new Dictionary<string, string>
        {
            { "General", "APHA_BATCH.GENERAL_EXCEPTION" },
            { "Sql", "APHA_BATCH.SQL_EXCEPTION" },
            { "Authorization", "APHA_BATCH.AUTHORIZATION_EXCEPTION" },
            { "Timeout", "APHA_BATCH.TIMEOUT_EXCEPTION" }
        };

    // Scan exception chain for specific exception types
    for (Exception? current = ex; current != null; current = current.InnerException)
    {
        if (current is NpgsqlException)
            return $"[[{exceptionTypes.GetValueOrDefault("Sql", "APHA_BATCH.SQL_EXCEPTION")}]]";

        if (current is TimeoutException)
            return $"[[{exceptionTypes.GetValueOrDefault("Timeout", "APHA_BATCH.TIMEOUT_EXCEPTION")}]]";

        if (current is UnauthorizedAccessException)
            return $"[[{exceptionTypes.GetValueOrDefault("Authorization", "APHA_BATCH.AUTHORIZATION_EXCEPTION")}]]";

        if (current is DbUpdateException)
            return $"[[{exceptionTypes.GetValueOrDefault("Sql", "APHA_BATCH.SQL_EXCEPTION")}]]";
    }

    // Default to general exception
    return $"[[{exceptionTypes.GetValueOrDefault("General", "APHA_BATCH.GENERAL_EXCEPTION")}]]";
}

static int ResolveIntSetting(IConfiguration configuration, string configKey, string envVarName, int defaultValue)
{
    var envValue = Environment.GetEnvironmentVariable(envVarName);
    if (!string.IsNullOrWhiteSpace(envValue) && int.TryParse(envValue, out var parsedEnv) && parsedEnv > 0)
    {
        return parsedEnv;
    }

    var configValue = configuration.GetValue<int?>(configKey);
    if (configValue.HasValue && configValue.Value > 0)
    {
        return configValue.Value;
    }

    return defaultValue;
}

static bool ResolveStrictExecutionContractMode(IConfiguration configuration, bool isLocalOrDevelopment)
{
    var envValue = Environment.GetEnvironmentVariable("BATCH_STRICT_EXECUTION_CONTRACT");
    if (!string.IsNullOrWhiteSpace(envValue) && bool.TryParse(envValue, out var parsedEnv))
    {
        return parsedEnv;
    }

    var configValue = configuration.GetValue<bool?>("BatchJobs:StrictExecutionContractMode");
    if (configValue.HasValue)
    {
        return configValue.Value;
    }

    // Safe default: strict outside local/development.
    return !isLocalOrDevelopment;
}

static DateTime? ParseRequestedAtUtc(string? requestedAtUtcRaw, Microsoft.Extensions.Logging.ILogger logger)
{
    if (string.IsNullOrWhiteSpace(requestedAtUtcRaw))
    {
        return null;
    }

    if (!DateTimeOffset.TryParse(
            requestedAtUtcRaw,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var parsedRequestedAtUtc))
    {
        logger.LogWarning(
            "Ignoring invalid BATCH_REQUESTED_AT_UTC value | RequestedAtUtc={RequestedAtUtc}",
            requestedAtUtcRaw);
        return null;
    }

    return parsedRequestedAtUtc.UtcDateTime;
}

static async Task VerifyExecutionContractAsync(
    string requestedJobName,
    Guid jobExecutionId,
    IJobExecutionRepository executionRepository,
    Microsoft.Extensions.Logging.ILogger logger,
    CancellationToken cancellationToken)
{
    var existingExecution = await executionRepository.GetExecutionByJobExecutionIdAsync(jobExecutionId, cancellationToken);
    if (existingExecution is null)
    {
        logger.LogInformation(
            "Execution contract pre-check passed | JobExecutionId={JobExecutionId} | JobName={JobName} | ExistingExecution=False",
            jobExecutionId,
            requestedJobName);
        return;
    }

    if (!string.Equals(existingExecution.JobName, requestedJobName, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            $"Execution contract violation: JobExecutionId '{jobExecutionId:D}' already belongs to job '{existingExecution.JobName}', not '{requestedJobName}'.");
    }

    var isTerminal = existingExecution.Status is JobStatus.Completed
        or JobStatus.Failed
        or JobStatus.Cancelled;

    if (isTerminal)
    {
        throw new InvalidOperationException(
            $"Execution contract violation: JobExecutionId '{jobExecutionId:D}' was already used by terminal execution '{existingExecution.Status}' (ExecutionId={existingExecution.ExecutionId}). Replays are not permitted.");
    }

    throw new InvalidOperationException(
        $"Execution contract violation: JobExecutionId '{jobExecutionId:D}' is already active with status '{existingExecution.Status}' (ExecutionId={existingExecution.ExecutionId}).");
}

