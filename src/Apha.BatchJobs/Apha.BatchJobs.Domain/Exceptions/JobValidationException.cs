namespace Apha.BatchJobs.Domain.Exceptions;

/// <summary>
/// Thrown when a batch job request fails pre-execution validation (e.g. missing
/// required parameters, invalid run mode, or unrecognised job name).
/// Maps to exit code <see cref="Constants.BatchExitCodes.ValidationFailure"/> (10).
/// </summary>
public sealed class JobValidationException : Exception
{
    public JobValidationException(string message) : base(message) { }

    public JobValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}
