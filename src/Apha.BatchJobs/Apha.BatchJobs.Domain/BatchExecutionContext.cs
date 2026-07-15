using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;

namespace Apha.BatchJobs.Domain;

/// <summary>
/// Immutable execution context resolved from environment variables at worker startup.
/// Validated at construction time — any missing or malformed value throws
/// <see cref="JobValidationException"/> before the host is fully built.
/// </summary>
public sealed record BatchExecutionContext
{
    /// <summary>Correlation ID that uniquely identifies this execution end-to-end.</summary>
    public Guid JobExecutionId { get; init; }

    /// <summary>Canonical job name (see <see cref="Constants.BatchJobNames"/>).</summary>
    public string JobName { get; init; } = default!;

    /// <summary>Execution trigger mode.</summary>
    public RunMode RunMode { get; init; }

    /// <summary>Identity of the requester (user ID, "system", or service account).</summary>
    public string RequestedBy { get; init; } = default!;

    /// <summary>UTC timestamp at which the execution was originally requested.</summary>
    public DateTimeOffset? RequestedAtUtc { get; init; }

    /// <summary>Optional JSON blob carrying job-specific parameters.</summary>
    public string? ParametersJson { get; init; }

    private BatchExecutionContext() { }

    /// <summary>
    /// Reads all required environment variables and returns a validated context.
    /// </summary>
    /// <exception cref="JobValidationException">
    /// Thrown when any required variable is missing, empty, or has an invalid format.
    /// </exception>
    public static BatchExecutionContext FromEnvironment()
    {
        var jobName = Environment.GetEnvironmentVariable("BATCH_JOB_NAME");
        if (string.IsNullOrWhiteSpace(jobName))
            throw new JobValidationException(
                "BATCH_JOB_NAME is not set. Verify the EventBridge input transformer maps $.detail.jobName → BATCH_JOB_NAME.");

        var requestedBy = Environment.GetEnvironmentVariable("BATCH_REQUESTED_BY") ?? "system";

        var runModeRaw = Environment.GetEnvironmentVariable("BATCH_RUN_MODE") ?? "Manual";
        if (!Enum.TryParse<RunMode>(runModeRaw, ignoreCase: true, out var runMode))
            throw new JobValidationException(
                $"BATCH_RUN_MODE value '{runModeRaw}' is not valid. Expected: Scheduled or Manual.");

        var jobExecutionIdRaw =
            Environment.GetEnvironmentVariable("BATCH_JOB_EXECUTION_ID")
            ?? Environment.GetEnvironmentVariable("BATCH_EXECUTION_ID");

        Guid jobExecutionId;
        if (string.IsNullOrWhiteSpace(jobExecutionIdRaw))
        {
            // Scheduled MABArchive is permitted to self-generate an execution ID.
            if (runMode == RunMode.Scheduled &&
                string.Equals(jobName, Constants.BatchJobNames.MabArchive, StringComparison.OrdinalIgnoreCase))
            {
                jobExecutionId = Guid.NewGuid();
            }
            else
            {
                throw new JobValidationException(
                    "BATCH_JOB_EXECUTION_ID is required for non-worker-managed runs.");
            }
        }
        else if (!Guid.TryParse(jobExecutionIdRaw, out jobExecutionId))
        {
            throw new JobValidationException(
                $"BATCH_JOB_EXECUTION_ID '{jobExecutionIdRaw}' is not a valid GUID.");
        }

        DateTimeOffset? requestedAtUtc = null;
        var requestedAtRaw = Environment.GetEnvironmentVariable("BATCH_REQUESTED_AT_UTC");
        if (!string.IsNullOrWhiteSpace(requestedAtRaw))
        {
            if (!DateTimeOffset.TryParse(requestedAtRaw,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal |
                    System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out var parsed))
            {
                throw new JobValidationException(
                    $"BATCH_REQUESTED_AT_UTC value '{requestedAtRaw}' is not a valid ISO-8601 timestamp.");
            }

            requestedAtUtc = parsed;
        }

        // For scheduled jobs fall back to now when caller did not supply a timestamp.
        if (requestedAtUtc == null && runMode == RunMode.Scheduled)
            requestedAtUtc = DateTimeOffset.UtcNow;

        var parametersJson = Environment.GetEnvironmentVariable("BATCH_PARAMETERS_JSON");
        if (!string.IsNullOrWhiteSpace(parametersJson))
        {
            // Validate it is parseable JSON to surface misconfiguration early.
            try
            {
                System.Text.Json.JsonDocument.Parse(parametersJson).Dispose();
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new JobValidationException(
                    $"BATCH_PARAMETERS_JSON is not valid JSON: {ex.Message}");
            }
        }

        return new BatchExecutionContext
        {
            JobExecutionId = jobExecutionId,
            JobName = jobName,
            RunMode = runMode,
            RequestedBy = requestedBy,
            RequestedAtUtc = requestedAtUtc,
            ParametersJson = string.IsNullOrWhiteSpace(parametersJson) ? null : parametersJson
        };
    }
}
