using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Domain.Enums;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class CreateProjectMonthFinalStepTests
{
    [SkippableFact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: In-memory EF Core context
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new BatchJobsDbContext(options);

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
        // Seed RsProjectMonth3
        db.RsProjectMonth3.Add(new RsProjectMonth3Table {
            EndPeriod = 1,
            PeriodName = "2026-01",
            Project = "P1",
            FpsYear = 2026,
            CumCost = 35m,
            CumInvoices = 15m,
            CumCoiw = 7m,
            CumPortSales = 2m,
            CumProfile = 100m,
            SumOfCostProfile = 100m,
            SumOfMstoneDue = 1d,
            SumOfDueDone = 1d,
            SumOfOnTime = 1d,
            CumCwDebit = 1m,
            CumCwCredit = 2m,
            CumTotalHours = 8d,
            CumSubContracts = 10d,
            CumTestCosts = 3d,
            CumPayCosts = 4d
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
        var step = new CreateProjectMonthFinalStep(1); // _month = 1
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("CreateProjectMonthFinal", result.StepName);
        Assert.Equal(StepStatus.Success, result.Status);

        // Validate output in RsProjectMonthFinal
        var rows = await db.RsProjectMonthFinal.ToListAsync();
        Assert.Single(rows);
        var row = rows[0];
        Assert.Equal("P1", row.Project);
        Assert.Equal(1, row.MonthNo);
        Assert.Equal(2026, row.FpsYear);
        Assert.Equal(100m, row.CostProfile);
        Assert.Equal(10m, row.SubContracts);
        Assert.Equal(5m, row.Animals);
        Assert.Equal(2m, row.NonAnimals);
        Assert.Equal(20m, row.TimeCosts); // decimal? cast
        Assert.Equal(3m, row.TransferCosts); // decimal? cast
        Assert.Equal(35m, row.TotalCost);
        Assert.Equal(15m, row.Invoices);
        Assert.Equal(7m, row.Coiw);
        Assert.Equal(2m, row.PortSales);
        Assert.Equal(35m, row.CumCost);
        Assert.Equal(100m, row.CumProfile);
        Assert.Equal("2026-01", row.PeriodName);
        Assert.Equal(100m, row.SumOfCostProfile);
        Assert.Equal(15m, row.CumInvoices);
        Assert.Equal(7m, row.CumCoiw);
        Assert.Equal(2m, row.CumPortSales);
        Assert.Equal(1d, row.MstoneDue);
        Assert.Equal(1d, row.DueDone);
        Assert.Equal(1d, row.OnTime);
        Assert.Equal(1d, row.SumOfMstoneDue);
        Assert.Equal(1d, row.SumOfDueDone);
        Assert.Equal(1d, row.SumOfOnTime);
        Assert.Equal(1, row.CumFlag);
        Assert.Equal(1m, row.CwDebit);
        Assert.Equal(2m, row.CwCredit);
        Assert.Equal(1m, row.CumCwDebit);
        Assert.Equal(2m, row.CumCwCredit);
        Assert.Equal(8d, row.TotalHours);
        Assert.Equal(8d, row.CumTotalHours);
        Assert.Equal(10d, row.CumSubContracts);
        Assert.Equal(3d, row.CumTestCosts);
        Assert.Equal(4d, row.PayCosts);
        Assert.Equal(4d, row.CumPayCosts);
    }
}
