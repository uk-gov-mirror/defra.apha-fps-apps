using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Domain.Constants;

namespace Apha.BatchJobs.UnitTests;

public sealed class BulkRatesExecutionContextTests
{
    // ── Create — identity comes from the caller, not the environment ────────

    [Fact]
    public void Create_ShouldUseSuppliedCorrelationIdAndJobName()
    {
        var id = Guid.NewGuid();

        var ctx = BulkRatesExecutionContext.Create(id.ToString("D"), BatchJobNames.BulkTestRatesUpdate);

        Assert.Equal(id, ctx.JobExecutionId);
        Assert.Equal(BatchJobNames.BulkTestRatesUpdate, ctx.JobName);
    }

    // ── Create — TriggerYear ──────────────────────────────────────────────

    [Fact]
    public void Create_WhenParametersJsonMissing_ShouldHaveNullTriggerYear()
    {
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", null);

        var ctx = BulkRatesExecutionContext.Create(Guid.NewGuid().ToString("D"), BatchJobNames.BulkTestRatesUpdate);

        Assert.Null(ctx.TriggerYear);
    }

    [Fact]
    public void Create_WhenParametersJsonContainsNumericYear_ShouldParseTriggerYear()
    {
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", "{\"year\":2027}");

        var ctx = BulkRatesExecutionContext.Create(Guid.NewGuid().ToString("D"), BatchJobNames.BulkTestRatesUpdate);

        Assert.Equal(2027, ctx.TriggerYear);
    }

    [Fact]
    public void Create_WhenParametersJsonContainsStringYear_ShouldParseTriggerYear()
    {
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", "{\"year\":\"2028\"}");

        var ctx = BulkRatesExecutionContext.Create(Guid.NewGuid().ToString("D"), BatchJobNames.BulkTestRatesUpdate);

        Assert.Equal(2028, ctx.TriggerYear);
    }

    [Fact]
    public void Create_WhenParametersJsonInvalid_ShouldHaveNullTriggerYear()
    {
        using var paramScope = new EnvScope("BATCH_JOB_PARAMETERS_JSON", "not-json");

        var ctx = BulkRatesExecutionContext.Create(Guid.NewGuid().ToString("D"), BatchJobNames.BulkTestRatesUpdate);

        Assert.Null(ctx.TriggerYear);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

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
