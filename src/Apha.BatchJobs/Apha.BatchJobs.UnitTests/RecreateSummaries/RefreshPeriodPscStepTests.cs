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

public sealed class RefreshPeriodPscStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        var period = 92;
        var project = harness.Id("PRJPSC");
        var program = "PRGPSC";
        var costCentre = Random.Shared.Next(700001, 799999);
        var subContCounter = Random.Shared.Next(980000, 989999);

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{program}', {harness.FpsYear}, 'charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('{project}', 'Project PSC', '{program}', 'Cust', 0::money, 0::money, 'Active', 'General',
                 'Contract', 0, {costCentre}, 'OPC1', 'SAC1', 'IA1', {harness.FpsYear});

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division)
            VALUES ('{harness.Id("PC1")}', 'Profit Centre', (SELECT divname FROM fps.tlkpdivision LIMIT 1));

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES ({costCentre}, '{harness.Id("PC1")}', {harness.FpsYear});

            INSERT INTO fps.proj_subcontract (subcontcounter, project, month, amount, acctcode, fpsyear)
            VALUES ({subContCounter}, '{project}', 1, 100::money, 'A1', {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var step = new RefreshPeriodPscStep(period);
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("RefreshPeriodPsc", result.StepName);
        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage);

        // Validate output in RsPeriodProjSubContract
        var row = await db.RsPeriodProjSubContract.AsNoTracking()
            .SingleAsync(x => x.Period == period && x.Project == project && x.SubContCounter == subContCounter);
        Assert.Equal(period, row.Period);
        Assert.Equal(subContCounter, row.SubContCounter);
        Assert.Equal(project, row.Project);
        Assert.Equal("OPC1", row.OracleProjectCode);
        Assert.Equal("SAC1", row.SubAccountCode);
        Assert.Equal("No", row.IsDefraProject);
        Assert.Equal(harness.Id("PC1"), row.Opc);
        Assert.Equal((double)costCentre, row.Occ ?? 0d);
        Assert.Equal(1, row.Month);
        Assert.Equal(100m, row.Amount);
        Assert.Equal("A1", row.AcctCode);
    }

    [Fact]
    public async Task ExecuteCoreAsync_IgnoresNullMonthRows()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        var period = 93;
        var project = harness.Id("PRJPSCNULL");
        var program = "PRGPSCN";
        var costCentre = Random.Shared.Next(700001, 799999);
        var subContCounter = Random.Shared.Next(970000, 979999);

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{program}', {harness.FpsYear}, 'charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('{project}', 'Project PSC Null Month', '{program}', 'Cust', 0::money, 0::money, 'Active', 'General',
                 'Contract', 0, {costCentre}, 'OPC2', 'SAC2', 'IA2', {harness.FpsYear});

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division)
            VALUES ('{harness.Id("PCN")}', 'Profit Centre', (SELECT divname FROM fps.tlkpdivision LIMIT 1));

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES ({costCentre}, '{harness.Id("PCN")}', {harness.FpsYear});

            INSERT INTO fps.proj_subcontract (subcontcounter, project, month, amount, acctcode, fpsyear)
            VALUES ({subContCounter}, '{project}', NULL, 125::money, 'A2', {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var step = new RefreshPeriodPscStep(period);

        var result = await step.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(StepStatus.Success, result.Status);

        var rows = await db.RsPeriodProjSubContract.AsNoTracking()
            .Where(x => x.Period == period && x.Project == project)
            .ToListAsync();

        Assert.Empty(rows);
    }
}
