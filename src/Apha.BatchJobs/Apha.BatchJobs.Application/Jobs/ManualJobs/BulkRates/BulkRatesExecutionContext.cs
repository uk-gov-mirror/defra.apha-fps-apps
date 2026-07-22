using System.Text.Json;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;

/// <summary>
/// Immutable execution context for BulkRates worker services.
/// JobExecutionId and JobName come from the execution request JobOrchestrator already resolved
/// for this run (the correlation id it set, and the job's own <c>Name</c>) rather than
/// re-parsing BATCH_JOB_EXECUTION_ID/BATCH_JOB_NAME from the environment a second time.
/// TriggerYear has no existing scoped source — it is job-specific payload data used only for
/// early mismatch detection, so it is still read from BATCH_JOB_PARAMETERS_JSON.
/// </summary>
public sealed record BulkRatesExecutionContext(
    Guid JobExecutionId,
    string JobName,
    int? TriggerYear)
{
    /// <summary>
    /// Builds the execution context from the already-resolved correlation id and job name.
    /// </summary>
    /// <param name="correlationId">
    /// The current execution's correlation id, set unconditionally by <c>JobOrchestrator</c>
    /// (as <c>jobExecutionId.ToString("D")</c>) before any job's <c>ExecuteAsync</c> runs.
    /// </param>
    /// <param name="jobName">The invoking job's own <c>Name</c>.</param>
    public static BulkRatesExecutionContext Create(string correlationId, string jobName)
    {
        var jobExecutionId = Guid.Parse(correlationId);
        var parametersJson = Environment.GetEnvironmentVariable("BATCH_JOB_PARAMETERS_JSON");
        var triggerYear = TryReadInt(parametersJson, "year");

        return new BulkRatesExecutionContext(jobExecutionId, jobName, triggerYear);
    }

    private static int? TryReadInt(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty(propertyName, out var value))
                return null;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var n))
                return n;

            if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var s))
                return s;
        }
        catch (JsonException)
        {
            // Non-critical; year from persisted record is authoritative.
        }

        return null;
    }
}
