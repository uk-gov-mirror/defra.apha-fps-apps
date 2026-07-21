using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Application.DependencyInjection;
using Apha.BatchJobs.Worker.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;

// Propagate CLI arg to env var so BatchExecutionContext.FromEnvironment() picks it up.
if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
    Environment.SetEnvironmentVariable("BATCH_JOB_NAME", args[0]);

var requestedJobArg = Environment.GetEnvironmentVariable("BATCH_JOB_NAME");

// A null/empty BATCH_JOB_NAME means the ECS container override did not inject the env var —
// most likely an EventBridge input transformer misconfiguration (e.g. PascalCase JSON keys
// not matching the camelCase paths $.detail.jobName expected by the transformer).
// Surface this as a configuration error rather than silently passing as HealthCheck.
if (string.IsNullOrWhiteSpace(requestedJobArg))
{
    Console.Error.WriteLine(
        "ERROR [ConfigurationError]: BATCH_JOB_NAME is not set. " +
        "Cannot determine which job to run. " +
        "Verify the EventBridge input transformer maps $.detail.jobName → BATCH_JOB_NAME.");
    return BatchExitCodes.ConfigurationFailure;
}

// HealthCheck is a pure process liveness probe — no DB, no orchestrator, no execution contract.
// The API must never create an Initiated record for HealthCheck invocations.
if (string.Equals(requestedJobArg, BatchJobNames.HealthCheck, StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("HealthCheck OK");
    return 0;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

var startedAt = DateTime.UtcNow;

// ECS SIGTERM → forced-stop window is typically 30 s.
// We allow 25 s for graceful cleanup before the host forces termination.
var gracefulShutdownWindowSeconds = ResolveIntSetting(
    builder.Configuration,
    "BatchJobs:GracefulShutdownWindowSeconds",
    "BATCH_GRACEFUL_SHUTDOWN_WINDOW_SECONDS",
    25);

// Extracted to extension — mirrors the pattern used by sibling API projects.
builder.ConfigureLogging();
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
var exitCode = BatchExitCodes.UnhandledFailure;
bool gracefulShutdownCompleted = true;

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

    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var dbCommandTimeoutSeconds = ResolveIntSetting(config, "BatchJobs:DbCommandTimeoutSeconds", "BATCH_DB_COMMAND_TIMEOUT_SECONDS", 30);
    var lockTimeoutSeconds = ResolveIntSetting(config, "BatchJobs:LockTimeoutSeconds", "BATCH_LOCK_TIMEOUT_SECONDS", 3600);
    var jobTimeoutSeconds = ResolveIntSetting(config, "BatchJobs:JobTimeout", "BATCH_JOB_TIMEOUT_SECONDS", 3600);

    logger.LogInformation("Flow checkpoint: Program.Main -> Host.Started -> Resolving JobOrchestrator");
    logger.LogInformation(
        "Runtime tolerance | GracefulShutdownWindowSeconds={GracefulShutdownWindowSeconds} | DbCommandTimeoutSeconds={DbCommandTimeoutSeconds} | LockTimeoutSeconds={LockTimeoutSeconds} | JobTimeoutSeconds={JobTimeoutSeconds}",
        gracefulShutdownWindowSeconds, dbCommandTimeoutSeconds, lockTimeoutSeconds, jobTimeoutSeconds);

    // Resolve and validate all execution inputs via BatchExecutionContext.
    var execContext = BatchExecutionContext.FromEnvironment();
    var jobName = execContext.JobName;
    var runMode = execContext.RunMode;
    var jobExecutionId = execContext.JobExecutionId;
    var userId = execContext.RequestedBy;
    var requestedAtUtc = execContext.RequestedAtUtc?.UtcDateTime;

    if (LooksLikeTemplatePlaceholder(jobName))
        throw new JobValidationException($"BATCH_JOB_NAME resolved to template placeholder '{jobName}'. Provide a real registered job name.");

    if (LooksLikeTemplatePlaceholder(userId))
        throw new JobValidationException($"BATCH_REQUESTED_BY resolved to template placeholder '{userId}'. Provide a real requester identity.");

    capturedJobExecutionId = jobExecutionId.ToString("D");
    requestedJobName = jobName;
    requestedRunMode = runMode.ToString();

    logger.LogInformation(
        "Requested job: {JobName} | RunMode: {RunMode} | JobExecutionId={JobExecutionId} | UserId={UserId} | RequestedAtUtc={RequestedAtUtc}",
        jobName, runMode, jobExecutionId, userId, requestedAtUtc?.ToString("O") ?? "n/a");

    // Cancel job execution only when the host is stopping.
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(hostLifetime.ApplicationStopping);

    if (hostLifetime.ApplicationStopping.IsCancellationRequested)
    {
        logger.LogWarning(
            "Host stopping signal was already set before job start — skipping execution | JobName={JobName} | GracefulWindowSeconds={GracefulWindowSeconds}",
            jobName, gracefulShutdownWindowSeconds);
        exitCode = BatchExitCodes.Cancelled;
        failureCategory = "Cancellation";
        runOutcome = "Cancelled";
    }
    else
    {
        await using var executionScope = serviceProvider.CreateAsyncScope();
        var orchestrator = executionScope.ServiceProvider.GetRequiredService<IJobOrchestrator>();
        var result = await orchestrator.RunAsync(jobName, runMode, jobExecutionId, userId, requestedAtUtc, linkedCts.Token);

        capturedJobQueueId = result.JobQueueId.ToString();
        capturedExecutionId = result.ExecutionId;

        logger.LogInformation("===========================================");
        logger.LogInformation(
            "Job '{JobName}' finished | Status={Status} | JobQueueId={JobQueueId} | ExecutionId={ExecutionId}",
            result.JobName, result.Status, result.JobQueueId, result.ExecutionId);
        logger.LogInformation("===========================================");

        exitCode = BatchExitCodes.Success;
        failureCategory = "None";
        runOutcome = "Succeeded";
    }
}
catch (JobValidationException ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    var configExceptionType = builder.Configuration["ExceptionTypes:Configuration"] ?? "FPSBatchJobs.CONFIGURATION_EXCEPTION";
    logger?.LogError(ex, "{ExceptionType} Configuration/validation error: {ErrorMessage}",
        $"[[{configExceptionType}]]",
        ex.Message);
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    exitCode = BatchExitCodes.ConfigurationFailure;
    failureCategory = "ConfigurationError";
    runOutcome = "Failed";
}
catch (InvalidOperationException ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    var exceptionPrefix = GetExceptionTypePrefix(ex, builder.Configuration);
    logger?.LogError(ex, "{ExceptionType} Job factory error: {ErrorMessage}", exceptionPrefix, ex.Message);
    Console.Error.WriteLine($"ERROR: {exceptionPrefix} {ex.Message}");
    exitCode = BatchExitCodes.ConfigurationFailure;
    failureCategory = "ConfigurationError";
    runOutcome = "Failed";
}
catch (OperationCanceledException ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    var remainingWindowMs = Math.Max(0, (int)(gracefulShutdownWindowSeconds * 1000 - (DateTime.UtcNow - startedAt).TotalMilliseconds));
    logger?.LogWarning(ex,
        "Job execution was interrupted | JobName={JobName} | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId} | RemainingShutdownWindowMs={RemainingWindowMs}",
        requestedJobName ?? "Unknown", capturedJobQueueId ?? "N/A", capturedJobExecutionId ?? "N/A", remainingWindowMs);
    Console.Error.WriteLine("INTERRUPTED: Job execution was interrupted");
    gracefulShutdownCompleted = remainingWindowMs > 100;
    exitCode = BatchExitCodes.Cancelled;
    failureCategory = "Cancellation";
    runOutcome = "Failed";
}
catch (Exception ex)
{
    var logger = loggerFactory?.CreateLogger("BatchJobs.Error");
    var exceptionPrefix = GetExceptionTypePrefix(ex, builder.Configuration);
    if (IsTimeoutFailure(ex))
    {
        logger?.LogError(ex, "{ExceptionType} Runtime timeout detected: {ErrorMessage}", exceptionPrefix, ex.Message);
        exitCode = BatchExitCodes.DatabaseFailure;
        failureCategory = "Timeout";
    }
    else if (IsDependencyOutage(ex))
    {
        logger?.LogError(ex, "{ExceptionType} Dependency outage detected: {ErrorMessage}", exceptionPrefix, ex.Message);
        exitCode = BatchExitCodes.DatabaseFailure;
        failureCategory = "DependencyOutage";
    }
    else
    {
        logger?.LogError(ex, "{ExceptionType} Unhandled exception: {ErrorMessage}", exceptionPrefix, ex.Message);
        exitCode = BatchExitCodes.UnhandledFailure;
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
            startedAt, endedAtUtc, runOutcome, failureCategory, exitCode, humanReadableMessage,
            requestedJobName ?? "Unknown", capturedJobQueueId ?? "N/A",
            capturedExecutionId?.ToString() ?? "N/A", capturedJobExecutionId ?? "N/A",
            requestedRunMode, durationMs, gracefulShutdownCompleted);

        Console.WriteLine(summaryLine);

        var logger = loggerFactory?.CreateLogger("BatchJobs.Summary");
        if (logLevel == LogLevel.Information)
            logger?.LogInformation(summaryTemplate, startedAt, endedAtUtc, runOutcome, failureCategory, exitCode, humanReadableMessage, requestedJobName ?? "Unknown", capturedJobQueueId ?? "N/A", capturedExecutionId?.ToString() ?? "N/A", capturedJobExecutionId ?? "N/A", requestedRunMode, durationMs, gracefulShutdownCompleted);
        else if (logLevel == LogLevel.Warning)
            logger?.LogWarning(summaryTemplate, startedAt, endedAtUtc, runOutcome, failureCategory, exitCode, humanReadableMessage, requestedJobName ?? "Unknown", capturedJobQueueId ?? "N/A", capturedExecutionId?.ToString() ?? "N/A", capturedJobExecutionId ?? "N/A", requestedRunMode, durationMs, gracefulShutdownCompleted);
        else
            logger?.LogError(summaryTemplate, startedAt, endedAtUtc, runOutcome, failureCategory, exitCode, humanReadableMessage, requestedJobName ?? "Unknown", capturedJobQueueId ?? "N/A", capturedExecutionId?.ToString() ?? "N/A", capturedJobExecutionId ?? "N/A", requestedRunMode, durationMs, gracefulShutdownCompleted);
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

return exitCode;

// ─── Helpers ────────────────────────────────────────────────────────────────

static string BuildSummaryLine(DateTime startedAt, DateTime endedAt, string outcome, string failureCategory,
    int exitCode, string message, string jobName, string jobQueueId, string executionId,
    string jobExecutionId, string runMode, double totalDurationMs, bool gracefulShutdownCompleted) =>
    $"Run completed | StartedAt={startedAt:O} | EndedAt={endedAt:O} | Outcome={outcome} | FailureCategory={failureCategory} | ExitCode={exitCode} | Message={message} | JobName={jobName} | JobQueueId={jobQueueId} | ExecutionId={executionId} | JobExecutionId={jobExecutionId} | RunMode={runMode} | TotalDurationMs={totalDurationMs:F0} | GracefulShutdownCompleted={gracefulShutdownCompleted}";

static string GenerateHumanReadableMessage(string outcome, string failureCategory) =>
    (outcome, failureCategory) switch
    {
        ("Succeeded", _) => "Job completed successfully within the graceful shutdown window.",
        ("Failed", "Cancellation") => "Job execution was interrupted due to host shutdown or timeout.",
        ("Failed", "Timeout") => "Job failed because execution exceeded the configured runtime timeout.",
        ("Failed", "ConfigurationError") => "Job failed due to configuration error (job not registered, invalid settings, etc.).",
        ("Failed", "BusinessFailure") => "Job failed with a business or runtime exception.",
        ("Failed", "DependencyOutage") => "Job failed due to dependency outage (database unavailable, network timeout, etc.).",
        _ => $"Job execution ended with outcome: {outcome} ({failureCategory})."
    };

static bool IsTimeoutFailure(Exception ex)
{
    for (var current = ex; current != null; current = current.InnerException)
        if (current is TimeoutException) return true;
    return false;
}

static bool IsDependencyOutage(Exception ex)
{
    for (var current = ex; current != null; current = current.InnerException)
        if (current is NpgsqlException || current is DbUpdateException) return true;
    return false;
}

static string GetExceptionTypePrefix(Exception ex, IConfiguration? config = null)
{
    var exceptionTypes = config?.GetSection("ExceptionTypes").Get<Dictionary<string, string>>()
        ?? new Dictionary<string, string>
        {
            { "General", "APHA_BATCH.GENERAL_EXCEPTION" },
            { "Sql", "APHA_BATCH.SQL_EXCEPTION" },
            { "Authorization", "APHA_BATCH.AUTHORIZATION_EXCEPTION" },
            { "Timeout", "APHA_BATCH.TIMEOUT_EXCEPTION" }
        };

    for (var current = ex; current != null; current = current.InnerException)
    {
        if (current is NpgsqlException) return $"[[{exceptionTypes.GetValueOrDefault("Sql", "APHA_BATCH.SQL_EXCEPTION")}]]";
        if (current is TimeoutException) return $"[[{exceptionTypes.GetValueOrDefault("Timeout", "APHA_BATCH.TIMEOUT_EXCEPTION")}]]";
        if (current is UnauthorizedAccessException) return $"[[{exceptionTypes.GetValueOrDefault("Authorization", "APHA_BATCH.AUTHORIZATION_EXCEPTION")}]]";
        if (current is DbUpdateException) return $"[[{exceptionTypes.GetValueOrDefault("Sql", "APHA_BATCH.SQL_EXCEPTION")}]]";
    }

    return $"[[{exceptionTypes.GetValueOrDefault("General", "APHA_BATCH.GENERAL_EXCEPTION")}]]";
}

static int ResolveIntSetting(IConfiguration configuration, string configKey, string envVarName, int defaultValue)
{
    var envValue = Environment.GetEnvironmentVariable(envVarName);
    if (!string.IsNullOrWhiteSpace(envValue) && int.TryParse(envValue, out var parsedEnv) && parsedEnv >= 0)
        return parsedEnv;

    var configValue = configuration.GetValue<int?>(configKey);
    if (configValue.HasValue && configValue.Value >= 0)
        return configValue.Value;

    return defaultValue;
}

static bool LooksLikeTemplatePlaceholder(string? value)
{
    if (string.IsNullOrWhiteSpace(value)) return false;
    var trimmed = value.Trim();
    return trimmed.Length > 2 && trimmed[0] == '<' && trimmed[^1] == '>';
}


