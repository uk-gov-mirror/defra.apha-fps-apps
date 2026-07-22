using Microsoft.Extensions.Hosting;

namespace Apha.BatchJobs.Worker.Lifecycle;

/// <summary>
/// Owns the cancellation token for one job execution: a configured overall-timeout CTS linked
/// with <see cref="IHostApplicationLifetime.ApplicationStopping"/>, plus first-shutdown-timestamp
/// capture so the runner can classify why the token was cancelled. Created and disposed inside
/// a single <c>BatchWorkerRunner.RunAsync</c> invocation — never registered in DI.
/// </summary>
public sealed class ExecutionCancellationContext : IDisposable
{
    private readonly CancellationTokenSource _timeoutCts;
    private readonly CancellationTokenSource _linkedCts;
    private readonly CancellationTokenRegistration _shutdownRegistration;
    private DateTime? _shutdownRequestedAtUtc;

    public ExecutionCancellationContext(IHostApplicationLifetime hostLifetime, int overallTimeoutSeconds)
    {
        ArgumentNullException.ThrowIfNull(hostLifetime);

        _timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(overallTimeoutSeconds));
        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_timeoutCts.Token, hostLifetime.ApplicationStopping);
        _shutdownRegistration = hostLifetime.ApplicationStopping.Register(() => _shutdownRequestedAtUtc ??= DateTime.UtcNow);
    }

    /// <summary>Linked token to pass to <c>IJobOrchestrator.RunAsync</c>.</summary>
    public CancellationToken Token => _linkedCts.Token;

    /// <summary>True once <see cref="IHostApplicationLifetime.ApplicationStopping"/> has fired, at any point.</summary>
    public bool WasHostShutdownRequested => _shutdownRequestedAtUtc.HasValue;

    /// <summary>
    /// True only when the overall timeout fired and host shutdown was never requested — host
    /// shutdown always takes precedence so an ECS SIGTERM is never misreported as a timeout.
    /// </summary>
    public bool WasJobTimeoutReached => _timeoutCts.IsCancellationRequested && !WasHostShutdownRequested;

    /// <summary>UTC timestamp of the first <see cref="IHostApplicationLifetime.ApplicationStopping"/> signal, if any.</summary>
    public DateTime? ShutdownRequestedAtUtc => _shutdownRequestedAtUtc;

    /// <summary>Classifies why <see cref="Token"/> was cancelled, for mapping an <see cref="OperationCanceledException"/>.</summary>
    public ExecutionCancellationReason ClassifyCancellation() => this switch
    {
        { WasHostShutdownRequested: true } => ExecutionCancellationReason.HostShutdown,
        { WasJobTimeoutReached: true } => ExecutionCancellationReason.Timeout,
        _ => ExecutionCancellationReason.Unclassified
    };

    public void Dispose()
    {
        _shutdownRegistration.Dispose();
        _linkedCts.Dispose();
        _timeoutCts.Dispose();
    }
}
