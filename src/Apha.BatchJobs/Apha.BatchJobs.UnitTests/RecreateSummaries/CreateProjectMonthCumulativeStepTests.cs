using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class CreateProjectMonthCumulativeStepTests
{
    [SkippableFact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Seed RsTblPeriod
        db.RsTblPeriod.Add(new RsTblPeriodTable { EndPeriod = 1, PeriodName = "2026-01", PeriodLocked = 0, FpsYear = 2026 });
        // Seed RsTblkPeriodMonth
        db.RsTblkPeriodMonth.Add(new RsTblkPeriodMonthTable { PeriodName = "2026-01", MonthNo = 1 });
        // Seed RsProjectMonth2
        db.RsProjectMonth2.Add(new RsProjectMonth2Table {
            Project = "P1",
            MonthNo = 1,
            FpsYear = 2026,
            CostProfile = 100m,
            SubContracts = 10m,
            Animals = 5m,
            NonAnimal = 2m,
            TimeCosts = 20d,
            TransferCosts = 3d,
            TotalCost = 35m,
            Invoices = 15m,
            Coiw = 7m,
            SumOfCostProfile = 100m,
            PortSales = 2d,
            MstoneDue = 1d,
            DueDone = 1d,
            OnTime = 1d,
            TotalHours = 8d,
            PayCosts = 4d
        });
        // Seed RsProjectMonthCasework
        var casework = new RsProjectMonthCaseworkTable {
            Project = "P1",
            MonthNo = 1,
            CwDebit = 1d,
            CwCredit = 2d
        };
        db.RsProjectMonthCasework.Add(casework);
        db.Entry(casework).Property("FpsYear").CurrentValue = 2026;
        await db.SaveChangesAsync();

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var step = new CreateProjectMonthCumulativeStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("CreateProjectMonthCumulative", result.StepName);

        // Validate output in RsProjectMonth3
        var rows = await db.RsProjectMonth3.ToListAsync();
        Assert.Single(rows);
        var row = rows[0];
        Assert.Equal(1, row.EndPeriod);
        Assert.Equal("2026-01", row.PeriodName);
        Assert.Equal("P1", row.Project);
        Assert.Equal(35m, row.CumCost);
        Assert.Equal(15m, row.CumInvoices);
        Assert.Equal(7m, row.CumCoiw);
        Assert.Equal(2m, row.CumPortSales);
        Assert.Equal(100m, row.CumProfile);
        Assert.Equal(100m, row.SumOfCostProfile);
        Assert.Equal(1d, row.SumOfMstoneDue);
        Assert.Equal(1d, row.SumOfDueDone);
        Assert.Equal(1d, row.SumOfOnTime);
        Assert.Equal(1m, row.CumCwDebit);
        Assert.Equal(2m, row.CumCwCredit);
        Assert.Equal(8d, row.CumTotalHours);
        Assert.Equal(10d, row.CumSubContracts); // SubContracts as double
        Assert.Equal(3d, row.CumTestCosts);
        Assert.Equal(4d, row.CumPayCosts);
    }
}
