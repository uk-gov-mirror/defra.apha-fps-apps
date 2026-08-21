using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;

namespace Apha.BatchJobs.Worker.Execution;

/// <summary>
/// Reads BATCH_JOB_* environment variables, validates them, and returns the immutable
/// <see cref="BatchExecutionRequest"/> the runner consumes.
/// </summary>
public sealed class BatchExecutionRequestResolver
{
    public BatchExecutionRequest Resolve()
    {
        var jobName = Environment.GetEnvironmentVariable("BATCH_JOB_NAME");
        if (string.IsNullOrWhiteSpace(jobName))
            throw new JobValidationException(
                "BATCH_JOB_NAME is not set. Verify the EventBridge input transformer maps $.detail.jobName → BATCH_JOB_NAME.");

        if (LooksLikeTemplatePlaceholder(jobName))
            throw new JobValidationException(
                $"BATCH_JOB_NAME resolved to template placeholder '{jobName}'. Provide a real registered job name.");

        var requestedBy = Environment.GetEnvironmentVariable("BATCH_REQUESTED_BY") ?? "system";

        if (LooksLikeTemplatePlaceholder(requestedBy))
            throw new JobValidationException(
                $"BATCH_REQUESTED_BY resolved to template placeholder '{requestedBy}'. Provide a real requester identity.");

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
            // Scheduled MABArchive and MilestoneUpdateNotifications are permitted to self-generate an execution ID.
            if (runMode == RunMode.Scheduled &&
                (string.Equals(jobName, BatchJobNames.MabArchive, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(jobName, BatchJobNames.MilestoneUpdateNotifications, StringComparison.OrdinalIgnoreCase)))
            {
                jobExecutionId = Guid.NewGuid();
                // Publish back so any subsequent read within this process observes the same value.
                Environment.SetEnvironmentVariable("BATCH_JOB_EXECUTION_ID", jobExecutionId.ToString("D"));
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

        var parametersJson = Environment.GetEnvironmentVariable("BATCH_JOB_PARAMETERS_JSON");
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
                    $"BATCH_JOB_PARAMETERS_JSON is not valid JSON: {ex.Message}");
            }
        }

        return new BatchExecutionRequest(
            jobName,
            runMode,
            jobExecutionId,
            requestedBy,
            requestedAtUtc?.UtcDateTime,
            string.IsNullOrWhiteSpace(parametersJson) ? null : parametersJson);
    }

    private static bool LooksLikeTemplatePlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        return trimmed.Length > 2 && trimmed[0] == '<' && trimmed[^1] == '>';
    }
}
