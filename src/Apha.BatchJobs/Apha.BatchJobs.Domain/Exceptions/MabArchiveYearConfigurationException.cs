namespace Apha.BatchJobs.Domain.Exceptions;

/// <summary>
/// Thrown when the FPS year configuration read from fps.tblyearmaster is invalid for
/// MABArchive processing (no Open year, more than one Open year, more than one Planned
/// year, or a Planned year that is not exactly Open year + 1). This is a business/
/// reference-data configuration failure, distinct from <see cref="JobValidationException"/>
/// (batch job request/parameter validation).
/// </summary>
public sealed class MabArchiveYearConfigurationException : Exception
{
    public MabArchiveYearConfigurationException(string message) : base(message) { }

    public MabArchiveYearConfigurationException(string message, Exception innerException)
        : base(message, innerException) { }
}
