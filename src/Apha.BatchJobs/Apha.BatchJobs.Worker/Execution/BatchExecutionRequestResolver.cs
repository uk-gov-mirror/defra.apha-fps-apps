using Apha.BatchJobs.Domain;
using Apha.BatchJobs.Domain.Exceptions;

namespace Apha.BatchJobs.Worker.Execution;

/// <summary>
/// Resolves and validates the current batch execution request. Most validation already lives in
/// <see cref="BatchExecutionContext.FromEnvironment"/> (missing/invalid job name, run mode,
/// execution id, requested-at timestamp, parameters JSON) — this resolver adds only the
/// template-placeholder rejection Program.cs used to do inline, then maps to the immutable
/// <see cref="BatchExecutionRequest"/> the runner consumes.
/// </summary>
public sealed class BatchExecutionRequestResolver
{
    public BatchExecutionRequest Resolve()
    {
        var context = BatchExecutionContext.FromEnvironment();

        if (LooksLikeTemplatePlaceholder(context.JobName))
        {
            throw new JobValidationException(
                $"BATCH_JOB_NAME resolved to template placeholder '{context.JobName}'. Provide a real registered job name.");
        }

        if (LooksLikeTemplatePlaceholder(context.RequestedBy))
        {
            throw new JobValidationException(
                $"BATCH_REQUESTED_BY resolved to template placeholder '{context.RequestedBy}'. Provide a real requester identity.");
        }

        return new BatchExecutionRequest(
            context.JobName,
            context.RunMode,
            context.JobExecutionId,
            context.RequestedBy,
            context.RequestedAtUtc?.UtcDateTime);
    }

    private static bool LooksLikeTemplatePlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        return trimmed.Length > 2 && trimmed[0] == '<' && trimmed[^1] == '>';
    }
}
