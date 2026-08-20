using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Worker.Execution;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="BatchExecutionRequestResolver"/>. HealthCheck is intentionally not
/// covered here â€” it never reaches this resolver (Program.cs short-circuits earlier).
/// </summary>
public sealed class BatchExecutionRequestResolverTests
{
    [Fact]
    public void Resolve_ValidManualRequest_ReturnsMatchingRequest()
    {
        var jobExecutionId = Guid.NewGuid();
        using var scope = new EnvScopeSet(
            jobName: "RecreateSummary",
            runMode: "Manual",
            jobExecutionId: jobExecutionId.ToString("D"),
            requestedBy: "arihant",
            requestedAtUtc: null);

        var request = new BatchExecutionRequestResolver().Resolve();

        Assert.Equal("RecreateSummary", request.JobName);
        Assert.Equal(RunMode.Manual, request.RunMode);
        Assert.Equal(jobExecutionId, request.JobExecutionId);
        Assert.Equal("arihant", request.RequestedBy);
        Assert.Null(request.RequestedAtUtc);
    }

    [Fact]
    public void Resolve_ValidScheduledRequest_ReturnsMatchingRequest()
    {
        var jobExecutionId = Guid.NewGuid();
        using var scope = new EnvScopeSet(
            jobName: "MABArchive",
            runMode: "Scheduled",
            jobExecutionId: jobExecutionId.ToString("D"),
            requestedBy: "scheduler",
            requestedAtUtc: "2026-07-22T03:00:00Z");

        var request = new BatchExecutionRequestResolver().Resolve();

        Assert.Equal("MABArchive", request.JobName);
        Assert.Equal(RunMode.Scheduled, request.RunMode);
        Assert.Equal(jobExecutionId, request.JobExecutionId);
        Assert.Equal("scheduler", request.RequestedBy);
        Assert.Equal(new DateTime(2026, 7, 22, 3, 0, 0, DateTimeKind.Utc), request.RequestedAtUtc);
    }

    [Fact]
    public void Resolve_WhenParametersJsonSet_ReturnsItOnTheRequest()
    {
        using var scope = new EnvScopeSet(
            jobName: "RecreateSummary",
            runMode: "Manual",
            jobExecutionId: Guid.NewGuid().ToString("D"),
            requestedBy: "arihant",
            requestedAtUtc: null,
            parametersJson: "{\"month\":\"2026-07\"}");

        var request = new BatchExecutionRequestResolver().Resolve();

        Assert.Equal("{\"month\":\"2026-07\"}", request.ParametersJson);
    }

    [Fact]
    public void Resolve_WhenParametersJsonNotSet_ReturnsNull()
    {
        using var scope = new EnvScopeSet(
            jobName: "RecreateSummary",
            runMode: "Manual",
            jobExecutionId: Guid.NewGuid().ToString("D"),
            requestedBy: "arihant",
            requestedAtUtc: null);

        var request = new BatchExecutionRequestResolver().Resolve();

        Assert.Null(request.ParametersJson);
    }

    [Fact]
    public void Resolve_WhenRequestedByMissing_DefaultsToSystem()
    {
        using var scope = new EnvScopeSet(
            jobName: "RecreateSummary",
            runMode: "Manual",
            jobExecutionId: Guid.NewGuid().ToString("D"),
            requestedBy: null,
            requestedAtUtc: null);

        var request = new BatchExecutionRequestResolver().Resolve();

        Assert.Equal("system", request.RequestedBy);
    }

    [Fact]
    public void Resolve_WhenJobNameIsTemplatePlaceholder_ThrowsJobValidationException()
    {
        using var scope = new EnvScopeSet(
            jobName: "<jobName>",
            runMode: "Manual",
            jobExecutionId: Guid.NewGuid().ToString("D"),
            requestedBy: "arihant",
            requestedAtUtc: null);

        var ex = Assert.Throws<JobValidationException>(() => new BatchExecutionRequestResolver().Resolve());
        Assert.Contains("BATCH_JOB_NAME", ex.Message);
    }

    [Fact]
    public void Resolve_WhenRequestedByIsTemplatePlaceholder_ThrowsJobValidationException()
    {
        using var scope = new EnvScopeSet(
            jobName: "RecreateSummary",
            runMode: "Manual",
            jobExecutionId: Guid.NewGuid().ToString("D"),
            requestedBy: "<userId>",
            requestedAtUtc: null);

        var ex = Assert.Throws<JobValidationException>(() => new BatchExecutionRequestResolver().Resolve());
        Assert.Contains("BATCH_REQUESTED_BY", ex.Message);
    }

    /// <summary>
    /// Sets every environment variable <see cref="Apha.BatchJobs.Domain.BatchExecutionContext.FromEnvironment"/>
    /// reads to a known state for the duration of one test, restoring the original values on dispose.
    /// </summary>
    private sealed class EnvScopeSet : IDisposable
    {
        private readonly List<EnvScope> _scopes = [];

        public EnvScopeSet(string? jobName, string? runMode, string? jobExecutionId, string? requestedBy, string? requestedAtUtc, string? parametersJson = null)
        {
            _scopes.Add(new EnvScope("BATCH_JOB_NAME", jobName));
            _scopes.Add(new EnvScope("BATCH_RUN_MODE", runMode));
            _scopes.Add(new EnvScope("BATCH_JOB_EXECUTION_ID", jobExecutionId));
            _scopes.Add(new EnvScope("BATCH_EXECUTION_ID", null));
            _scopes.Add(new EnvScope("BATCH_REQUESTED_BY", requestedBy));
            _scopes.Add(new EnvScope("BATCH_REQUESTED_AT_UTC", requestedAtUtc));
            _scopes.Add(new EnvScope("BATCH_JOB_PARAMETERS_JSON", parametersJson));
        }

        public void Dispose()
        {
            foreach (var scope in _scopes)
                scope.Dispose();
        }
    }

    private sealed class EnvScope : IDisposable
    {
        private readonly string _name;
        private readonly string? _original;

        public EnvScope(string name, string? value)
        {
            _name = name;
            _original = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose() => Environment.SetEnvironmentVariable(_name, _original);
    }
}
