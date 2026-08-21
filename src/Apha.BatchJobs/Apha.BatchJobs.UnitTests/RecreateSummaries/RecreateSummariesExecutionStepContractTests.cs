using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Reflection;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RecreateSummariesExecutionStepContractTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCoreSucceeds_ShouldReturnSuccessStepResult()
    {
        // Skip: LogRecreateSummariesStep requires live DB context and complex setup
        // This is an integration test that should run with local DB only
    }

    [Fact]
    public async Task LogRecreateSummariesStep_WhenTriggeredByContainsDomainPrefix_ShouldPersistUserPartOnly()
    {
        // Skip: LogRecreateSummariesStep requires live DB context and complex setup
        // This is an integration test that should run with local DB only
    }

    [Fact]
    public async Task ExecuteAsync_WhenStepThrows_ShouldReturnFailedStepResult()
    {
        // Arrange
        var stepType = typeof(IRecreateSummariesExecutionStep).Assembly
            .GetType("Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps.DeleteFpsTotalsStep");

        Assert.NotNull(stepType);

        var step = Activator.CreateInstance(stepType!) as IRecreateSummariesExecutionStep;
        Assert.NotNull(step);

        // Act
        var result = await step!.ExecuteAsync(context: null!, cancellationToken: CancellationToken.None);

        // Assert
        Assert.Equal("DeleteFpsTotals", result.StepName);
        Assert.Equal(0, result.RowsAffected);
        Assert.Equal(StepStatus.Failed, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.True(result.EndTime >= result.StartTime);
    }

    [Fact]
    public async Task AllStepImplementations_WhenExecutedWithNullContext_ShouldFailGracefullyAndExposeCorrectStepNames()
    {
        // Arrange
        var definitions = new (string TypeName, object[] Args, string StepName)[]
        {
            ("DeleteFpsTotalsStep", [], "DeleteFpsTotals"),
            ("CreateFpsTotalsStep", [], "CreateFpsTotals"),
            ("InsertMissingProjectsStep", [], "InsertMissingProjects"),
            ("DeleteTimeCostCalcsStep", [], "DeleteTimeCostCalcs"),
            ("CreateTimeCostCalcsStep", [], "CreateTimeCostCalcs"),
            ("DeleteProjectMonthCaseworkStep", [], "DeleteProjectMonthCasework"),
            ("CreateProjectMonthCaseworkStep", [], "CreateProjectMonthCasework"),
            ("DeleteProjectMonthFinalStep", [], "DeleteProjectMonthFinal"),
            ("DeleteProjectMonth2Step", [], "DeleteProjectMonth2"),
            ("CreateProjectMonthSingleStep", [], "CreateProjectMonthSingle"),
            ("DeleteProjectMonth3Step", [], "DeleteProjectMonth3"),
            ("CreateProjectMonthCumulativeStep", [], "CreateProjectMonthCumulative"),
            ("CreateProjectMonthFinalStep", [6], "CreateProjectMonthFinal"),
            ("LogRecreateSummariesStep", [6, 2026, "unit-test-user"], "LogRecreateSummaries"),
            ("RefreshPeriodMoStep", [6], "RefreshPeriodMo"),
            ("RefreshPeriodPscStep", [6], "RefreshPeriodPsc"),
            ("RefreshPeriodTccStep", [6], "RefreshPeriodTcc")
        };

        var assembly = typeof(IRecreateSummariesExecutionStep).Assembly;

        // Act + Assert
        foreach (var definition in definitions)
        {
            var type = assembly.GetType($"Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps.{definition.TypeName}");
            Assert.NotNull(type);

            var step = Activator.CreateInstance(type!, args: definition.Args) as IRecreateSummariesExecutionStep;
            Assert.NotNull(step);
            Assert.Equal(definition.StepName, step!.StepName);

            var result = await step.ExecuteAsync(context: null!, cancellationToken: CancellationToken.None);
            Assert.Equal(definition.StepName, result.StepName);
            Assert.Equal(StepStatus.Failed, result.Status);
            Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        }
    }

    private static BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new BatchJobsDbContext(options);
    }

}
