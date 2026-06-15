using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Pact.Api.Models;
using Apha.BatchJobs.Pact.Api.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Apha.BatchJobs.Pact.Api.Services;

public sealed class LocalWorkerProcessTriggerDispatcher : ITriggerDispatcher
{
    private readonly TriggerDispatchOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalWorkerProcessTriggerDispatcher> _logger;
    private readonly ITriggerAttemptStore _triggerAttemptStore;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public LocalWorkerProcessTriggerDispatcher(
        IOptions<TriggerDispatchOptions> options,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory,
        ITriggerAttemptStore triggerAttemptStore,
        ILogger<LocalWorkerProcessTriggerDispatcher> logger)
    {
        _options = options.Value;
        _environment = environment;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
        _triggerAttemptStore = triggerAttemptStore;
        _logger = logger;
    }

    public Task<string> DispatchAsync(BatchTriggerEventDetail detail, CancellationToken cancellationToken = default)
    {
        var workerProjectPath = ResolveWorkerProjectPath();
        if (!File.Exists(workerProjectPath))
        {
            throw new FileNotFoundException($"Worker project not found at '{workerProjectPath}'.");
        }

        var shouldPauseForDebugger = _options.LocalWorker.WaitForDebuggerAttach;

        var workingDirectory = ResolveWorkingDirectory(workerProjectPath);
        var startInfo = new ProcessStartInfo
        {
            FileName = string.IsNullOrWhiteSpace(_options.LocalWorker.DotnetExecutable)
                ? "dotnet"
                : _options.LocalWorker.DotnetExecutable,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(workerProjectPath);

        var workerEnvironment = string.IsNullOrWhiteSpace(_options.LocalWorker.WorkerEnvironmentName)
            ? _environment.EnvironmentName
            : _options.LocalWorker.WorkerEnvironmentName;

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = workerEnvironment;
        startInfo.Environment["DOTNET_ENVIRONMENT"] = workerEnvironment;
        startInfo.Environment["BATCH_JOB_NAME"] = detail.JobName;
        startInfo.Environment["BATCH_RUN_MODE"] = detail.RunMode;
        startInfo.Environment["BATCH_JOB_EXECUTION_ID"] = detail.JobExecutionId;
        startInfo.Environment["BATCH_REQUESTED_BY"] = detail.RequestedBy;
        startInfo.Environment["BATCH_REQUESTED_AT_UTC"] = detail.RequestedAtUtc.ToString("O");
        startInfo.Environment["BATCH_JOB_PARAMETERS_JSON"] = string.IsNullOrWhiteSpace(detail.ParametersJson) ? "{}" : detail.ParametersJson;

        if (string.Equals(detail.JobName, "RecreateSummaries", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(detail.ParametersJson)
                && TryExtractRecreateSummariesMonth(detail.ParametersJson, out var year, out var month))
            {
                startInfo.Environment["BATCH_RECREATE_SUMMARIES_MONTH"] = month.ToString();
                startInfo.Environment["BATCH_RECREATE_SUMMARIES_YEAR"] = year.ToString();
            }
        }

        startInfo.Environment["BATCH_DEBUG_WAIT_FOR_ATTACH"] = shouldPauseForDebugger ? "true" : "false";
        startInfo.Environment["BATCH_DEBUG_ATTACH_TIMEOUT_SECONDS"] = _options.LocalWorker.DebuggerAttachTimeoutSeconds.ToString();
        startInfo.Environment["BATCH_DEBUG_BREAK_ON_START"] =
            shouldPauseForDebugger && _options.LocalWorker.BreakOnStartAfterAttach ? "true" : "false";

        if (Guid.TryParse(detail.JobExecutionId, out var jobQueueGuid))
        {
            startInfo.Environment["BATCH_JOBQUEUE_ID"] = jobQueueGuid.ToString("D");
        }

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start local worker process.");

        var logFilePath = BuildWorkerProcessLogFilePath(detail, process.Id);
        var metadataPath = LocalWorkerProcessRegistry.BuildMetadataFilePath(_environment.ContentRootPath, process.Id);
        LocalWorkerProcessRegistry.Register(
            process.Id,
            detail.JobName,
            detail.JobExecutionId,
            DateTime.UtcNow,
            metadataPath,
            _logger);
        _ = Task.Run(() => ObserveProcessAsync(process, logFilePath, metadataPath, detail.JobExecutionId));

        _logger.LogInformation(
            "Local worker dispatch started | JobName={JobName} | JobExecutionId={JobExecutionId} | WorkerPid={WorkerPid} | WorkerProjectPath={WorkerProjectPath} | WorkerEnvironment={WorkerEnvironment} | WaitForDebuggerAttach={WaitForDebuggerAttach} | WorkerLogFilePath={WorkerLogFilePath}",
            detail.JobName,
            detail.JobExecutionId,
            process.Id,
            workerProjectPath,
            workerEnvironment,
            shouldPauseForDebugger,
            logFilePath);

        return Task.FromResult($"localproc-{process.Id}");
    }

    private string BuildWorkerProcessLogFilePath(BatchTriggerEventDetail detail, int workerPid)
    {
        var logsRoot = Path.Combine(_environment.ContentRootPath, "Logs", "LocalWorkerProcess");
        Directory.CreateDirectory(logsRoot);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var safeJobName = string.Concat(detail.JobName.Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_'));
        if (string.IsNullOrWhiteSpace(safeJobName))
        {
            safeJobName = "UnknownJob";
        }

        var fileName = $"{timestamp}-{safeJobName}-{detail.JobExecutionId}-pid{workerPid}.log";
        return Path.Combine(logsRoot, fileName);
    }

    private async Task ObserveProcessAsync(Process process, string logFilePath, string metadataPath, string jobExecutionId)
    {
        try
        {
            await using var file = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            await using var writer = new StreamWriter(file, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true
            };

            var includeSqlLogs = _options.LocalWorker.IncludeSqlCommandLogs;
            var outputTask = PumpReaderAsync(process.StandardOutput, writer, "OUT", includeSqlLogs);
            var errorTask = PumpReaderAsync(process.StandardError, writer, "ERR", includeSqlLogs);

            await process.WaitForExitAsync();
            await Task.WhenAll(outputTask, errorTask);

            var failureReason = process.ExitCode == 0
                ? null
                : TryExtractFailureReasonFromLog(logFilePath);

            await UpdateTriggerAttemptOnExitAsync(jobExecutionId, process.ExitCode, failureReason, logFilePath);
            await UpdateExecutionStatusOnExitAsync(jobExecutionId, process.ExitCode, failureReason);

            await writer.WriteLineAsync($"[{DateTime.UtcNow:O}] [SYS] Worker process exited with code {process.ExitCode}");

            _logger.LogInformation(
                "Local worker process exited | WorkerPid={WorkerPid} | ExitCode={ExitCode} | WorkerLogFilePath={WorkerLogFilePath}",
                process.Id,
                process.ExitCode,
                logFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to observe local worker process output | WorkerPid={WorkerPid} | WorkerLogFilePath={WorkerLogFilePath}",
                process.Id,
                logFilePath);
        }
        finally
        {
            LocalWorkerProcessRegistry.Unregister(process.Id, metadataPath, _logger);
            process.Dispose();
        }
    }

    private async Task UpdateTriggerAttemptOnExitAsync(string jobExecutionId, int exitCode, string? failureReason, string logFilePath)
    {
        var existing = await _triggerAttemptStore.GetByJobExecutionIdAsync(jobExecutionId);
        if (existing is null)
        {
            return;
        }

        failureReason ??= exitCode == 0 ? null : TryExtractFailureReasonFromLog(logFilePath);

        var mappedStatus = exitCode switch
        {
            0 => "Completed",
            3 => "Cancelled",
            4 => "Skipped",
            _ => "Failed"
        };

        await _triggerAttemptStore.SaveAsync(
            new TriggerAttemptRecord
            {
                JobExecutionId = existing.JobExecutionId,
                JobName = existing.JobName,
                AcceptedAtUtc = existing.AcceptedAtUtc,
                EventId = existing.EventId,
                WorkerProcessLaunched = existing.WorkerProcessLaunched,
                Status = mappedStatus,
                WorkerExitCode = exitCode,
                FailureReason = failureReason,
                StoredAtUtc = DateTime.UtcNow
            });
    }

    private async Task UpdateExecutionStatusOnExitAsync(string jobExecutionId, int exitCode, string? failureReason)
    {
        if (exitCode == 0)
        {
            return;
        }

        if (!Guid.TryParse(jobExecutionId, out var parsedJobExecutionId))
        {
            return;
        }

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var executionRepository = scope.ServiceProvider.GetRequiredService<IJobExecutionRepository>();
            var execution = await executionRepository.GetExecutionByJobExecutionIdAsync(parsedJobExecutionId);
            if (execution is null)
            {
                return;
            }

            if (execution.Status is JobStatus.Completed or JobStatus.Failed or JobStatus.Cancelled)
            {
                return;
            }

            execution.Status = exitCode == 3 ? JobStatus.Cancelled : JobStatus.Failed;
            execution.CompletedAt = DateTime.UtcNow;
            execution.ErrorMessage = string.IsNullOrWhiteSpace(failureReason)
                ? $"Local worker exited with code {exitCode} before reporting terminal status."
                : failureReason;

            if (string.IsNullOrWhiteSpace(execution.UserId))
            {
                execution.UserId = "pact.api-local-dispatcher";
            }

            await executionRepository.UpdateExecutionRecordAsync(execution);

            _logger.LogWarning(
                "Persisted terminal execution status after local worker exit | JobExecutionId={JobExecutionId} | ExitCode={ExitCode} | MappedStatus={MappedStatus}",
                jobExecutionId,
                exitCode,
                execution.Status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to persist terminal execution status after local worker exit | JobExecutionId={JobExecutionId} | ExitCode={ExitCode}",
                jobExecutionId,
                exitCode);
        }
    }

    private static string? TryExtractFailureReasonFromLog(string logFilePath)
    {
        try
        {
            if (!File.Exists(logFilePath))
            {
                return null;
            }

            var lines = File.ReadAllLines(logFilePath);
            for (var i = lines.Length - 1; i >= 0; i--)
            {
                var line = lines[i];
                if (line.Contains("FATAL ERROR:", StringComparison.OrdinalIgnoreCase))
                {
                    return line[(line.IndexOf("FATAL ERROR:", StringComparison.OrdinalIgnoreCase) + "FATAL ERROR:".Length)..].Trim();
                }

                if (line.Contains("Unhandled business/runtime exception:", StringComparison.OrdinalIgnoreCase))
                {
                    return line[(line.IndexOf("Unhandled business/runtime exception:", StringComparison.OrdinalIgnoreCase) + "Unhandled business/runtime exception:".Length)..].Trim();
                }
            }

            return lines.Length > 0 ? lines[^1] : null;
        }
        catch
        {
            return null;
        }
    }

    private static async Task PumpReaderAsync(StreamReader reader, StreamWriter writer, string streamTag, bool includeSqlLogs)
    {
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                return;
            }

            if (!includeSqlLogs && IsSqlNoiseLine(line))
            {
                continue;
            }

            // stdout/stderr are pumped in parallel; StreamWriter is not thread-safe for concurrent writes.
            lock (writer)
            {
                writer.WriteLine($"[{DateTime.UtcNow:O}] [{streamTag}] {line}");
                writer.Flush();
            }
        }
    }

    private static bool IsSqlNoiseLine(string line)
    {
        if (line.Contains("Executed DbCommand", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return line.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("INSERT ", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("UPDATE ", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("DELETE ", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("VALUES (", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("FROM ", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("WHERE ", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("LIMIT ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryExtractRecreateSummariesMonth(string parametersJson, out int year, out int month)
    {
        year = 0;
        month = 0;

        try
        {
            using var doc = JsonDocument.Parse(parametersJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!doc.RootElement.TryGetProperty("month", out var monthElement)
                || monthElement.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var value = monthElement.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var parts = value.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2
                || !int.TryParse(parts[0], out year)
                || !int.TryParse(parts[1], out month)
                || year is < 2000 or > 9999
                || month is < 1 or > 12)
            {
                return false;
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private string ResolveWorkerProjectPath()
    {
        var relativePath = _options.LocalWorker.WorkerProjectPath;
        if (Path.IsPathRooted(relativePath))
        {
            return relativePath;
        }

        var probeRoot = new DirectoryInfo(_environment.ContentRootPath);
        while (probeRoot is not null)
        {
            var candidatePath = Path.GetFullPath(relativePath, probeRoot.FullName);
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }

            probeRoot = probeRoot.Parent;
        }

        return Path.GetFullPath(relativePath, _environment.ContentRootPath);
    }

    private string ResolveWorkingDirectory(string workerProjectPath)
    {
        if (!string.IsNullOrWhiteSpace(_options.LocalWorker.WorkingDirectory))
        {
            return Path.IsPathRooted(_options.LocalWorker.WorkingDirectory)
                ? _options.LocalWorker.WorkingDirectory
                : Path.GetFullPath(_options.LocalWorker.WorkingDirectory, _environment.ContentRootPath);
        }

        return Path.GetDirectoryName(workerProjectPath) ?? _environment.ContentRootPath;
    }
}

internal static class LocalWorkerProcessRegistry
{
    private const string ActiveFolderName = "Active";

    public static string BuildMetadataFilePath(string contentRootPath, int pid)
    {
        var directory = GetMetadataDirectory(contentRootPath);
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"pid-{pid}.json");
    }

    public static void Register(
        int pid,
        string jobName,
        string jobExecutionId,
        DateTime startedAtUtc,
        string metadataPath,
        ILogger logger)
    {
        try
        {
            var record = new LocalWorkerProcessRecord
            {
                Pid = pid,
                ParentPid = Environment.ProcessId,
                JobName = jobName,
                JobExecutionId = jobExecutionId,
                StartedAtUtc = startedAtUtc
            };

            var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(metadataPath, json);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to write local worker process metadata | WorkerPid={WorkerPid} | MetadataPath={MetadataPath}",
                pid,
                metadataPath);
        }
    }

    public static void Unregister(int pid, string metadataPath, ILogger logger)
    {
        try
        {
            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to delete local worker process metadata | WorkerPid={WorkerPid} | MetadataPath={MetadataPath}",
                pid,
                metadataPath);
        }
    }

    public static string GetMetadataDirectory(string contentRootPath)
    {
        return Path.Combine(contentRootPath, "Logs", "LocalWorkerProcess", ActiveFolderName);
    }
}

internal sealed class LocalWorkerProcessRecord
{
    public int Pid { get; init; }

    public int ParentPid { get; init; }

    public string JobName { get; init; } = string.Empty;

    public string JobExecutionId { get; init; } = string.Empty;

    public DateTime StartedAtUtc { get; init; }
}
