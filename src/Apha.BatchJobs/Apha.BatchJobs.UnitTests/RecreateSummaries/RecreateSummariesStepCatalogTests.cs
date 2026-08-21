using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RecreateSummariesStepCatalogTests
{
    [Fact]
    public void BuildMandatorySteps_ShouldReturnStrictExpectedOrder()
    {
        // Arrange
        var subject = CreateCatalog();

        // Act
        var steps = subject.BuildMandatorySteps(month: 6, year: 2026, triggeredBy: "unit-test-user");

        // Assert
        var orderedStepNames = steps.Select(s => s.StepName).ToArray();
        var expected = new[]
        {
            "DeleteFpsTotals",
            "CreateFpsTotals",
            "InsertMissingProjects",
            "DeleteTimeCostCalcs",
            "CreateTimeCostCalcs",
            "DeleteProjectMonthCasework",
            "CreateProjectMonthCasework",
            "DeleteProjectMonthFinal",
            "DeleteProjectMonth2",
            "CreateProjectMonthSingle",
            "DeleteProjectMonth3",
            "CreateProjectMonthCumulative",
            "CreateProjectMonthFinal",
            "LogRecreateSummaries"
        };

        Assert.Equal(expected, orderedStepNames);

        // Explicit guard for dependency chain strictness.
        var idxSingle = Array.IndexOf(orderedStepNames, "CreateProjectMonthSingle");
        var idxCumulative = Array.IndexOf(orderedStepNames, "CreateProjectMonthCumulative");
        var idxFinal = Array.IndexOf(orderedStepNames, "CreateProjectMonthFinal");

        Assert.True(idxSingle >= 0, "CreateProjectMonthSingle must exist.");
        Assert.True(idxCumulative >= 0, "CreateProjectMonthCumulative must exist.");
        Assert.True(idxFinal >= 0, "CreateProjectMonthFinal must exist.");
        Assert.True(idxSingle < idxCumulative && idxCumulative < idxFinal,
            "Expected strict order: CreateProjectMonthSingle -> CreateProjectMonthCumulative -> CreateProjectMonthFinal.");
    }

    [Fact]
    public void BuildRefreshSteps_ShouldReturnExpectedOrder()
    {
        // Arrange
        var subject = CreateCatalog();

        // Act
        var steps = subject.BuildRefreshSteps(month: 6);

        // Assert
        var orderedStepNames = steps.Select(s => s.StepName).ToArray();

        Assert.Equal(
            new[] { "RefreshPeriodMo", "RefreshPeriodPsc", "RefreshPeriodTcc" },
            orderedStepNames);
    }

    private static IRecreateSummariesStepCatalog CreateCatalog()
    {
        var type = typeof(IRecreateSummariesStepCatalog).Assembly
            .GetType("Apha.BatchJobs.Infrastructure.RecreateSummaries.RecreateSummariesStepCatalog");

        Assert.NotNull(type);

        var instance = Activator.CreateInstance(type!, nonPublic: true) as IRecreateSummariesStepCatalog;

        Assert.NotNull(instance);
        return instance!;
    }
}
