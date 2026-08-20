using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.HealthCheck;
using Apha.BatchJobs.Domain.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// BatchJobFactory has no fallback path: it only resolves job types discovered by reflection
/// over the real Apha.BatchJobs.Application assembly (see ConventionalJobMap), so these tests
/// exercise it against a real, dependency-light job (HealthCheckJobHandler) rather than fakes
/// registered ad hoc in a test-local ServiceCollection.
/// </summary>
public sealed class BatchJobFactoryTests
{
    [Fact]
    public void Create_ShouldResolveRegisteredJob()
    {
        var services = new ServiceCollection();
        services.AddSingleton<HealthCheckJobHandler>();
        services.AddSingleton<ILogger<HealthCheckJobHandler>>(NullLogger<HealthCheckJobHandler>.Instance);
        services.AddSingleton<IOptions<BatchJobSettings>>(Options.Create(new BatchJobSettings()));
        using var serviceProvider = services.BuildServiceProvider();

        var factory = new BatchJobFactory(serviceProvider);

        var job = factory.Create("HealthCheck");

        Assert.IsType<HealthCheckJobHandler>(job);
        Assert.Equal("HealthCheck", job.Name);
    }

    [Fact]
    public void Create_ShouldThrowForUnknownJob()
    {
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var factory = new BatchJobFactory(serviceProvider);

        var action = () => factory.Create("MissingJob");

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("MissingJob", exception.Message);
    }

    [Fact]
    public void Create_ShouldThrowImmediately_WhenRegisteredTypeCannotBeResolvedFromContainer()
    {
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        // ConventionalJobMap finds HealthCheckJobHandler by reflection, but nothing here
        // registers it (or its dependencies) in the container â€” this must fail loudly at
        // this Create() call rather than silently falling back to constructing every other job.
        var factory = new BatchJobFactory(serviceProvider);

        var action = () => factory.Create("HealthCheck");

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("HealthCheck", exception.Message);
        Assert.Contains("could not be resolved", exception.Message);
    }

    [Fact]
    public void GetAvailableJobs_ShouldReturnKnownConventionalNames()
    {
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var factory = new BatchJobFactory(serviceProvider);

        // Regression guard: this is the full set of real IBatchJob types discovered by
        // reflection today. Adding, removing, or renaming a job should force a deliberate
        // update here rather than pass silently.
        var expected = new[]
        {
            "BulkAnimalRatesUpdate",
            "BulkStaffRatesUpdate",
            "BulkTestRatesUpdate",
            "HealthCheck",
            "MABArchive",
            "MilestoneUpdateNotifications",
            "RecreateSummary",
            "YearEndCutover",
            "YearEndDataSetup"
        };

        Assert.Equal(expected, factory.GetAvailableJobs());
    }

    [Fact]
    public void BuildConventionalJobMap_ShouldThrowForCollidingConventionalNames()
    {
        // FooJob -> "Foo" and FooJobHandler -> "Foo": two different type names that collide
        // on the same stripped conventional name must fail loudly at map-build time, not
        // silently drop both entries.
        var action = () => BatchJobFactory.BuildConventionalJobMap(
            new[] { typeof(FooJob), typeof(FooJobHandler) });

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("Foo", exception.Message);
        Assert.Contains(nameof(FooJob), exception.Message);
        Assert.Contains(nameof(FooJobHandler), exception.Message);
    }

    [Fact]
    public void BuildConventionalJobMap_ShouldMapDistinctNamesWithoutError()
    {
        var map = BatchJobFactory.BuildConventionalJobMap(new[] { typeof(FooJob), typeof(BarJobHandler) });

        Assert.Equal(typeof(FooJob), map["Foo"]);
        Assert.Equal(typeof(BarJobHandler), map["Bar"]);
    }

    private sealed class FooJob : IBatchJob
    {
        public string Name => "Foo";
        public string IdempotencyStrategy => "Upsert";

        public Task ExecuteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FooJobHandler : IBatchJob
    {
        public string Name => "Foo";
        public string IdempotencyStrategy => "Upsert";

        public Task ExecuteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class BarJobHandler : IBatchJob
    {
        public string Name => "Bar";
        public string IdempotencyStrategy => "Upsert";

        public Task ExecuteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
