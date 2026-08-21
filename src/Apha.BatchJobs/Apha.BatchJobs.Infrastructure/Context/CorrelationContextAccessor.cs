using Apha.BatchJobs.Domain.Interfaces;

namespace Apha.BatchJobs.Infrastructure.Context;

/// <summary>
/// AsyncLocal-based correlation ID service.
/// </summary>
public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<string?> CorrelationIdHolder = new();

    /// <inheritdoc />
    public string? GetCorrelationId() => CorrelationIdHolder.Value;

    /// <inheritdoc />
    public void SetCorrelationId(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation ID cannot be null or empty.", nameof(correlationId));
        }

        CorrelationIdHolder.Value = correlationId;
    }

    /// <inheritdoc />
    public string GenerateCorrelationId()
    {
        var correlationId = Guid.NewGuid().ToString("N");
        CorrelationIdHolder.Value = correlationId;
        return correlationId;
    }
}
