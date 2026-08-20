using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class InsertMissingProjectsStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Seed RsTlkpProject with two projects, one missing in RsProjectMonth for month 1
        db.RsTlkpProject.Add(new RsTlkpProjectTable {
            ParentProject = "P1",
            FpsYear = 2026
        });
        db.RsTlkpProject.Add(new RsTlkpProjectTable {
            ParentProject = "P2",
            FpsYear = 2026
        });
        db.RsProjectMonth.Add(new RsProjectMonthTable {
            Project = "P1",
            MonthNo = 1,
            FpsYear = 2026
        });
        await db.SaveChangesAsync();

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var step = new InsertMissingProjectsStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("InsertMissingProjects", result.StepName);
        Assert.True(result.Status == Apha.BatchJobs.Domain.Enums.StepStatus.Success, result.ErrorMessage);

        // Validate that P2 is now present for month 1 in execution year.
        var rows = await db.RsProjectMonth.ToListAsync();
        Assert.Contains(rows, r => r.Project == "P2" && r.MonthNo == 1 && r.FpsYear == 2026);
    }
}
