namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Contract for correlation ID management during batch job execution.
/// </summary>
public interface ICorrelationContextAccessor
{
    /// <summary>
    /// Gets the current correlation ID, or null when none is set.
    /// </summary>
    string? GetCorrelationId();

    /// <summary>
    /// Sets the correlation ID for the current execution flow.
    /// </summary>
    /// <param name="correlationId">Correlation ID value.</param>
    void SetCorrelationId(string correlationId);

    /// <summary>
    /// Generates and stores a new correlation ID.
    /// </summary>
    /// <returns>The generated correlation ID.</returns>
    string GenerateCorrelationId();
}
