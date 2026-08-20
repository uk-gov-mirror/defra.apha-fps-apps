using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Domain.Enums;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class DeleteProjectMonthCaseworkStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;
        var project = harness.Id("P1");

        // Seed RsProjectMonthCasework
        var casework = new RsProjectMonthCaseworkTable {
            Project = project,
            MonthNo = 1,
            CwDebit = 1d,
            CwCredit = 2d
        };
        db.RsProjectMonthCasework.Add(casework);
        db.Entry(casework).Property("FpsYear").CurrentValue = 2026;
        await db.SaveChangesAsync();

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var step = new DeleteProjectMonthCaseworkStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("DeleteProjectMonthCasework", result.StepName);
        Assert.Equal(StepStatus.Success, result.Status);
        Assert.False(await db.RsProjectMonthCasework.AsNoTracking().AnyAsync(x => x.Project == project));
    }
}
