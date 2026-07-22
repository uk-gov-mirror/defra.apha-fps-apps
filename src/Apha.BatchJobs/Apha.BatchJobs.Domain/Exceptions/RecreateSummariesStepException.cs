namespace Apha.BatchJobs.Domain.Exceptions;

/// <summary>
/// Thrown when a single RecreateSummaries business step (steps 1–17) fails. Carries no inner
/// exception because <c>RecreateSummariesExecutionStepBase</c> already reduces step failures to a
/// string <c>ErrorMessage</c> before returning a <see cref="Enums.StepStatus.Failed"/> result.
/// Caught exactly once at the RecreateSummaries job's transaction boundary, which performs the
/// single rollback for the whole run before rethrowing.
/// </summary>
public sealed class RecreateSummariesStepException : Exception
{
    public string StepName { get; }

    public RecreateSummariesStepException(string stepName, string? errorMessage)
        : base($"RecreateSummaries step '{stepName}' failed: {errorMessage}")
    {
        StepName = stepName;
    }
}
