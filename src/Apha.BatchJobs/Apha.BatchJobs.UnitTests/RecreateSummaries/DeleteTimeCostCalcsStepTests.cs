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

public sealed class DeleteTimeCostCalcsStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;
        var project = harness.Id("P1");

        // Seed RsTimeCostCalcs
        db.RsTimeCostCalcs.Add(new RsTimeCostCalcsTable {
            WorkGroup = "WG1",
            JobCode = "JC1",
            Project = project,
            Month = 1,
            StaffId = "S1",
            FpsYear = 2026
        });
        await db.SaveChangesAsync();

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var step = new DeleteTimeCostCalcsStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("DeleteTimeCostCalcs", result.StepName);
        Assert.Equal(StepStatus.Success, result.Status);
        Assert.False(await db.RsTimeCostCalcs.AsNoTracking().AnyAsync(x => x.Project == project));
    }
}
