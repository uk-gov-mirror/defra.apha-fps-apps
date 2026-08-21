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

public sealed class CreateProjectMonthSingleStepTests
{
    [SkippableFact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        var project = harness.Id("P1");
        var program = "PRS1";
        var testCode = harness.Id("T1");
        var workGroup = harness.Id("WG1");
        var profitCentre = harness.Id("PC1");
        var costCentre = Random.Shared.Next(700001, 799999);

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{program}', {harness.FpsYear}, 'charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, isdefraproject, incomeaccountcode, fpsyear)
            VALUES
                ('{project}', 'Project PMS', '{program}', 'Cust', 0::money, 0::money, 'Active', 'General',
                 'Contract', 0, 'IA1', {harness.FpsYear});

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division)
            VALUES ('{profitCentre}', 'Profit Centre', (SELECT divname FROM fps.tlkpdivision LIMIT 1));

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES ({costCentre}, '{profitCentre}', {harness.FpsYear});

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('{workGroup}', '{profitCentre}', {costCentre}, {harness.FpsYear});

            INSERT INTO fps.projectmonth (project, monthno, costprofile, fpsyear)
            VALUES ('{project}', 1, 100::money, {harness.FpsYear});

            INSERT INTO fps.proj_subcontract (subcontcounter, project, month, amount, acctcode, fpsyear)
            VALUES
                (910001, '{project}', 1, 5::money, 'LargeAnimals', {harness.FpsYear}),
                (910002, '{project}', 1, 5::money, 'Other', {harness.FpsYear});

            INSERT INTO fps.timecostcalcs
                (workgroup, jobcode, project, month, staffid, name, chargerate, time, cost, pay, nonpay, overhead, fpsyear)
            VALUES
                ('{workGroup}', '{harness.Id("JC")}', '{project}', 1, '{harness.Id("S1")}', 'Staff', 1::money, 8, 20, 4::money, 0::money, 0::money, {harness.FpsYear});

            INSERT INTO fps.milestone
                (project, milestoneref, objectiveref, plandate, actualdate, monthnofin, year, fpsyear)
            VALUES
                ('{project}', 'MS1', 'OBJ1', DATE '2026-01-10', DATE '2026-01-10', 1, '2026', {harness.FpsYear});

            INSERT INTO fps.proj_invoice (projectparent, month, amount, costofwork, invoicecounter, fpsyear)
            VALUES ('{project}', 1, 15::money, 7::money, 920001, {harness.FpsYear});

            INSERT INTO fps.testorproduct (itemcode, owner, jobstatus, defraunitprice, fpsyear)
            VALUES ('{testCode}', 'PA', 'AC', 1::money, {harness.FpsYear});

            INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, active, fpsyear)
            VALUES ('{testCode}', '{project}', 3::money, 1, '{project}', 1, {harness.FpsYear});

            INSERT INTO fps.tlkptestcapability (testcode, workgroup, planportfolio, fpsyear)
            VALUES ('{testCode}', '{workGroup}', '{project}', {harness.FpsYear});

            INSERT INTO fps.monthlyoutput (buyer, workgroup, testcode, month, volume, fpsyear)
            VALUES ('{project}', '{workGroup}', '{testCode}', 1, 1, {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var deleteStep = new DeleteProjectMonth2Step();
        var deleteResult = await deleteStep.ExecuteAsync(context, CancellationToken.None);
        Assert.True(deleteResult.Status == StepStatus.Success, deleteResult.ErrorMessage);

        var step = new CreateProjectMonthSingleStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("CreateProjectMonthSingle", result.StepName);
        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage);

        // Validate output in RsProjectMonth2
        var row = await db.RsProjectMonth2.AsNoTracking()
            .SingleAsync(x => x.Project == project && x.MonthNo == 1 && x.FpsYear == harness.FpsYear);
        Assert.Equal(project, row.Project);
        Assert.Equal(1, row.MonthNo);
        Assert.Equal(harness.FpsYear, row.FpsYear);
        Assert.Equal(100m, row.CostProfile);
        Assert.Equal(10m, row.SubContracts);
        Assert.Equal(5m, row.Animals);
        Assert.Equal(5m, row.NonAnimal);
        Assert.Equal(20d, row.TimeCosts);
        Assert.Equal(3d, row.TransferCosts);
        Assert.Equal(10m + 20m + 3m, row.TotalCost); // SubContracts + TimeCosts + TransferCosts
        Assert.Equal(15m, row.Invoices);
        Assert.Equal(7m, row.Coiw);
        Assert.Equal(100m, row.SumOfCostProfile);
        Assert.Equal(3d, row.PortSales);
        Assert.True((row.MstoneDue ?? 0d) >= 0d);
        Assert.True((row.DueDone ?? 0d) >= 0d);
        Assert.True((row.OnTime ?? 0d) >= 0d);
        Assert.Equal(8d, row.TotalHours);
        Assert.Equal(4d, row.PayCosts ?? 0d);
    }
}
