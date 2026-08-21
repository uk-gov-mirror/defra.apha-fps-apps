using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apha.BatchJobs.UnitTests;

public sealed class YearEndDataSetupServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCurrentYearMissing_ShouldThrow()
    {
        var service = CreateService(new ValidateYearEndContextStep());
        var context = new YearEndExecutionContext("corr-1", null, CurrentFpsYear: null, TargetFpsYear: 2027);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains("currentFpsYear", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetYearNotGreaterThanCurrent_ShouldThrow()
    {
        var service = CreateService(new ValidateYearEndContextStep());
        var context = new YearEndExecutionContext("corr-2", null, CurrentFpsYear: 2027, TargetFpsYear: 2027);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains("targetFpsYear", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidationPassesAndPlaceholderExists_ShouldFailAtPlaceholder()
    {
        var service = CreateService(
            new ValidateYearEndContextStep(),
            new PlaceholderYearEndDataSetupStep("ValidateYearScopedSchemaStep"));

        var context = new YearEndExecutionContext(
            CorrelationId: "corr-3",
            ParametersJson: "{\"currentFpsYear\":2026,\"targetFpsYear\":2027}",
            CurrentFpsYear: 2026,
            TargetFpsYear: 2027);

        var ex = await Assert.ThrowsAsync<NotImplementedException>(() => service.ExecuteAsync(context));

        Assert.Contains("ValidateYearScopedSchemaStep", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExecuteRegisteredStepsInOrder()
    {
        var tracker = new List<string>();
        var service = CreateService(
            new RecordingStep("StepA", tracker),
            new RecordingStep("StepB", tracker));

        var context = new YearEndExecutionContext("corr-4", null, 2026, 2027);

        await service.ExecuteAsync(context);

        Assert.Equal(new[] { "StepA", "StepB" }, tracker);
    }

    private static YearEndDataSetupService CreateService(params IYearEndDataSetupStep[] steps)
    {
        return new YearEndDataSetupService(steps, NullLogger<YearEndDataSetupService>.Instance);
    }

    private sealed class RecordingStep : IYearEndDataSetupStep
    {
        private readonly List<string> _tracker;

        public RecordingStep(string name, List<string> tracker)
        {
            Name = name;
            _tracker = tracker;
        }

        public string Name { get; }

        public Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
        {
            _tracker.Add(Name);
            return Task.CompletedTask;
        }
    }
}
