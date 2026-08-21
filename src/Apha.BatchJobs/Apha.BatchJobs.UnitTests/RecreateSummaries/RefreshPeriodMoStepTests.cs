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

public sealed class RefreshPeriodMoStepTests
{
    [SkippableFact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        var period = 91;
        var project = harness.Id("PRJMO");
        var program = "PRGMO";
        var costCentre = Random.Shared.Next(700001, 799999);
        var workGroup = harness.Id("WG1");
        var testCode = harness.Id("T1");

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{program}', {harness.FpsYear}, 'charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('{project}', 'Project MO', '{program}', 'Cust', 0::money, 0::money, 'Active', 'General',
                 'Contract', 0, {costCentre}, 'OPC1', 'SAC1', 'IA1', {harness.FpsYear});

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division)
            VALUES ('{harness.Id("PC1")}', 'Profit Centre', (SELECT divname FROM fps.tlkpdivision LIMIT 1));

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES ({costCentre}, '{harness.Id("PC1")}', {harness.FpsYear});

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('{workGroup}', '{harness.Id("PC1")}', {costCentre}, {harness.FpsYear});

            INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, active, fpsyear)
            VALUES ('{testCode}', '{project}', 10::money, 1, '{project}', 1, {harness.FpsYear});

            INSERT INTO fps.monthlyoutput (buyer, workgroup, testcode, month, volume, fpsyear)
            VALUES ('{project}', '{workGroup}', '{testCode}', 1, 5, {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var step = new RefreshPeriodMoStep(period);
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("RefreshPeriodMo", result.StepName);
        Assert.Equal(StepStatus.Success, result.Status);

        // Validate output in RsPeriodMonthlyOutput
        var row = await db.RsPeriodMonthlyOutput.AsNoTracking()
            .SingleAsync(x => x.Period == period && x.Project == project && x.Month == 1);
        Assert.Equal(period, row.Period);
        Assert.Equal(project, row.Project);
        Assert.Equal("OPC1", row.OracleProjectCode);
        Assert.Equal("SAC1", row.SubAccountCode);
        Assert.Equal("No", row.IsDefraProject);
        Assert.Equal(harness.Id("PC1"), row.Opc);
        Assert.Equal((double)costCentre, row.Occ ?? 0d);
        Assert.Equal(1, row.Month);
        Assert.Equal(harness.Id("PC1"), row.Spc);
        Assert.Equal(workGroup, row.WorkGroup);
        Assert.Equal((double)costCentre, row.Scc ?? 0d);
        Assert.Equal(testCode, row.TestCode);
        Assert.Equal(5, row.Volume);
        Assert.Equal(10m, row.TestPrice);
        Assert.Equal(50m, row.TotalCost);
    }

    [SkippableFact]
    public async Task ExecuteCoreAsync_IgnoresRowsFromOtherYears()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        var period = 999;
        var currentProject = harness.Id("PRJMOCUR");
        var priorProject = harness.Id("PRJMOOLD");
        var currentProgram = "PRGMO1";
        var priorProgram = "PRGMO2";
        var currentCostCentre = Random.Shared.Next(700001, 799999);
        var priorCostCentre = Random.Shared.Next(600001, 699999);
        var currentWorkGroup = harness.Id("WG2");
        var priorWorkGroup = harness.Id("WG3");
        var currentTestCode = harness.Id("T2");
        var priorTestCode = harness.Id("T3");

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{currentProgram}', {harness.FpsYear}, 'charge'),
                   ('{priorProgram}', {harness.FpsYear - 1}, 'charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('{currentProject}', 'Project MO Current', '{currentProgram}', 'Cust', 0::money, 0::money, 'Active', 'General',
                 'Contract', 0, {currentCostCentre}, 'OPC-CUR', 'SAC-CUR', 'IA-CUR', {harness.FpsYear}),
                ('{priorProject}', 'Project MO Prior', '{priorProgram}', 'Cust', 0::money, 0::money, 'Active', 'General',
                 'Contract', 0, {priorCostCentre}, 'OPC-OLD', 'SAC-OLD', 'IA-OLD', {harness.FpsYear - 1});

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division)
            VALUES ('{harness.Id("PC2")}', 'Profit Centre Current', (SELECT divname FROM fps.tlkpdivision LIMIT 1)),
                   ('{harness.Id("PC3")}', 'Profit Centre Prior', (SELECT divname FROM fps.tlkpdivision LIMIT 1));

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES ({currentCostCentre}, '{harness.Id("PC2")}', {harness.FpsYear}),
                   ({priorCostCentre}, '{harness.Id("PC3")}', {harness.FpsYear - 1});

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
                 VALUES ('{currentWorkGroup}', '{harness.Id("PC2")}', {currentCostCentre}, {harness.FpsYear}),
                     ('{priorWorkGroup}', '{harness.Id("PC3")}', {priorCostCentre}, {harness.FpsYear - 1});

            INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, active, fpsyear)
                 VALUES ('{currentTestCode}', '{currentProject}', 10::money, 1, '{currentProject}', 1, {harness.FpsYear}),
                     ('{priorTestCode}', '{priorProject}', 10::money, 1, '{priorProject}', 1, {harness.FpsYear - 1});

            INSERT INTO fps.monthlyoutput (buyer, workgroup, testcode, month, volume, fpsyear)
                 VALUES ('{currentProject}', '{currentWorkGroup}', '{currentTestCode}', 1, 5, {harness.FpsYear}),
                     ('{priorProject}', '{priorWorkGroup}', '{priorTestCode}', 1, 7, {harness.FpsYear - 1});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), harness.FpsYear);
        var step = new RefreshPeriodMoStep(period);

        var result = await step.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(StepStatus.Success, result.Status);

        var row = await db.RsPeriodMonthlyOutput.AsNoTracking()
            .SingleAsync(x => x.Period == period && x.Project == currentProject);

        Assert.Equal("OPC-CUR", row.OracleProjectCode);
        Assert.Equal("SAC-CUR", row.SubAccountCode);
        Assert.Equal(harness.Id("PC2"), row.Opc);
        Assert.Equal((double)currentCostCentre, row.Occ ?? 0d);
        Assert.Equal(currentWorkGroup, row.WorkGroup);
        Assert.Equal(currentTestCode, row.TestCode);

        var priorRows = await db.RsPeriodMonthlyOutput.AsNoTracking()
            .Where(x => x.Period == period && x.Project == priorProject)
            .ToListAsync();

        Assert.Empty(priorRows);
    }

    [SkippableFact]
    public async Task ExecuteCoreAsync_DoesNotMultiplyRowsFromOtherYearSources()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        var period = 1000;
        var project = harness.Id("PRJMOSRC");
        var program = "PRGMOS";
        var workGroupCurrent = harness.Id("WG4");
        var workGroupPrior = harness.Id("WG5");
        var testCode = harness.Id("T4");
        var currentCostCentre = Random.Shared.Next(700001, 799999);

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{program}', {harness.FpsYear}, 'charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('{project}', 'Project MO Source Scope', '{program}', 'Cust', 0::money, 0::money, 'Active', 'General',
                 'Contract', 0, {currentCostCentre}, 'OPC-SRC', 'SAC-SRC', 'IA-SRC', {harness.FpsYear});

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division)
            VALUES ('{harness.Id("PC4")}', 'Profit Centre Source', (SELECT divname FROM fps.tlkpdivision LIMIT 1));

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES ({currentCostCentre}, '{harness.Id("PC4")}', {harness.FpsYear});

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('{workGroupCurrent}', '{harness.Id("PC4")}', {currentCostCentre}, {harness.FpsYear}),
                   ('{workGroupPrior}', '{harness.Id("PC4")}', {currentCostCentre}, {harness.FpsYear - 1});

            INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, active, fpsyear)
            VALUES ('{testCode}', '{project}', 12::money, 1, '{project}', 1, {harness.FpsYear}),
                   ('{testCode}', '{project}', 33::money, 1, '{project}', 1, {harness.FpsYear - 1});

            INSERT INTO fps.monthlyoutput (buyer, workgroup, testcode, month, volume, fpsyear)
            VALUES ('{project}', '{workGroupCurrent}', '{testCode}', 1, 4, {harness.FpsYear}),
                   ('{project}', '{workGroupPrior}', '{testCode}', 1, 9, {harness.FpsYear - 1});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), harness.FpsYear);
        var step = new RefreshPeriodMoStep(period);

        var result = await step.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(StepStatus.Success, result.Status);

        var rows = await db.RsPeriodMonthlyOutput.AsNoTracking()
            .Where(x => x.Period == period && x.Project == project)
            .ToListAsync();

        Assert.Single(rows);
        Assert.Equal(4d, rows[0].Volume ?? 0d);
        Assert.Equal(12m, rows[0].TestPrice ?? 0m);
    }
}
