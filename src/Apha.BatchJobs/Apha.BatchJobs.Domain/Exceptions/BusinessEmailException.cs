namespace Apha.BatchJobs.Domain.Exceptions;

/// <summary>
/// Thrown when a business notification email cannot be sent (e.g. SMTP failure,
/// invalid recipient, or email service unavailable).
/// Maps to exit code <see cref="Constants.BatchExitCodes.EmailFailure"/> (50).
/// </summary>
public sealed class BusinessEmailException : Exception
{
    public BusinessEmailException(string message) : base(message) { }

    public BusinessEmailException(string message, Exception innerException)
        : base(message, innerException) { }
}
