using Apha.BatchJobs.Worker.Lifecycle;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="ExecutionCancellationContext"/> â€” the overall-timeout CTS linked with
/// host shutdown, with host shutdown always taking precedence when both fire.
/// </summary>
public sealed class ExecutionCancellationContextTests
{
    private static IHostApplicationLifetime CreateLifetime(out CancellationTokenSource lifetimeCts)
    {
        lifetimeCts = new CancellationTokenSource();
        var hostLifetime = Substitute.For<IHostApplicationLifetime>();
        hostLifetime.ApplicationStopping.Returns(lifetimeCts.Token);
        return hostLifetime;
    }

    [Fact]
    public async Task WasJobTimeoutReached_WhenOnlyTimeoutFires_IsTrueAndShutdownIsFalse()
    {
        var hostLifetime = CreateLifetime(out _);
        using var context = new ExecutionCancellationContext(hostLifetime, overallTimeoutSeconds: 1);

        await Task.Delay(TimeSpan.FromSeconds(1.5));

        Assert.True(context.Token.IsCancellationRequested);
        Assert.True(context.WasJobTimeoutReached);
        Assert.False(context.WasHostShutdownRequested);
        Assert.Null(context.ShutdownRequestedAtUtc);
        Assert.Equal(ExecutionCancellationReason.Timeout, context.ClassifyCancellation());
    }

    [Fact]
    public void WasHostShutdownRequested_WhenOnlyApplicationStoppingFires_IsTrueAndTimeoutIsFalse()
    {
        var hostLifetime = CreateLifetime(out var lifetimeCts);
        using var context = new ExecutionCancellationContext(hostLifetime, overallTimeoutSeconds: 3600);

        lifetimeCts.Cancel();

        Assert.True(context.Token.IsCancellationRequested);
        Assert.True(context.WasHostShutdownRequested);
        Assert.False(context.WasJobTimeoutReached);
        Assert.NotNull(context.ShutdownRequestedAtUtc);
        Assert.Equal(ExecutionCancellationReason.HostShutdown, context.ClassifyCancellation());
    }

    [Fact]
    public async Task ClassifyCancellation_WhenBothTimeoutAndShutdownFire_PrefersHostShutdown()
    {
        var hostLifetime = CreateLifetime(out var lifetimeCts);
        using var context = new ExecutionCancellationContext(hostLifetime, overallTimeoutSeconds: 1);

        await Task.Delay(TimeSpan.FromSeconds(1.5));
        lifetimeCts.Cancel();

        Assert.True(context.WasHostShutdownRequested);
        Assert.False(context.WasJobTimeoutReached);
        Assert.Equal(ExecutionCancellationReason.HostShutdown, context.ClassifyCancellation());
    }

    [Fact]
    public void ClassifyCancellation_WhenNeitherFires_IsUnclassified()
    {
        var hostLifetime = CreateLifetime(out _);
        using var context = new ExecutionCancellationContext(hostLifetime, overallTimeoutSeconds: 3600);

        Assert.False(context.Token.IsCancellationRequested);
        Assert.Equal(ExecutionCancellationReason.Unclassified, context.ClassifyCancellation());
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var hostLifetime = CreateLifetime(out _);
        var context = new ExecutionCancellationContext(hostLifetime, overallTimeoutSeconds: 3600);

        var exception = Record.Exception(context.Dispose);

        Assert.Null(exception);
    }
}
