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

public sealed class RefreshPeriodTccStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        var period = 93;
        var project = harness.Id("PRJTCC");
        var program = "PRGTCC";
        var costCentre = Random.Shared.Next(700001, 799999);
        var pcGrade = harness.Id("PCG1");
        var wgGrade = harness.Id("WG1");
        var spNumber = "SP123";
        var workGroup = harness.Id("WG1");
        var staffId = harness.Id("S1");

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{program}', {harness.FpsYear}, 'charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('{project}', 'Project TCC', '{program}', 'Cust', 0::money, 0::money, 'Active', 'General',
                 'Contract', 0, {costCentre}, 'OPC1', 'SAC1', 'IA1', {harness.FpsYear});

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division)
            VALUES ('{harness.Id("PC1")}', 'Profit Centre', (SELECT divname FROM fps.tlkpdivision LIMIT 1));

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES ({costCentre}, '{harness.Id("PC1")}', {harness.FpsYear});

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('{workGroup}', '{harness.Id("PC1")}', {costCentre}, {harness.FpsYear});

            INSERT INTO fps.profitcentregrade
                (pcgrade, divisiongrade, gradecode, profitcentre, chargerate, payrate, defrachargerate, fpsyear)
            VALUES
                ('{pcGrade}', 'D1', 'GC1', '{harness.Id("PC1")}', 1::money, 1::money, 1::money, {harness.FpsYear});

            INSERT INTO fps.workgroupgrade
                (wggrade, profitcentregrade, gradecode, workgroup, fpsyear)
            VALUES
                ('{wgGrade}', '{pcGrade}', 'GC1', '{workGroup}', {harness.FpsYear});

            INSERT INTO fps.tblemployee (spnumber, firstname, lastname, fpsyear)
            VALUES ('{spNumber}', 'Staff', 'One', {harness.FpsYear});

            INSERT INTO fps.tblwgemployee
                (pactid, spnumber, workgroupgrade, personstatus, personclass, hrspaid, leave, sickspecial, hrsavail,
                 makeavailable, timerecorder, fpsyear)
            VALUES
                ('{staffId}', '{spNumber}', '{wgGrade}', 'A', 'P', 37, 0, 0, 37, 1, 0, {harness.FpsYear});

            INSERT INTO fps.timecostcalcs
                (workgroup, jobcode, project, month, staffid, gradecode, name, chargerate, class, time, cost,
                 division, pay, nonpay, overhead, fpsyear)
            VALUES
                ('{workGroup}', '{harness.Id("JC1")}', '{project}', 1, '{staffId}', 'GC1', 'Staff1', 10::money, 'Charge', 8, 80,
                 'DivA', 40::money, 16::money, 8::money, {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var step = new RefreshPeriodTccStep(period);
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("RefreshPeriodTcc", result.StepName);
        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage);

        // Validate output in RsPeriodTimeCostCalcs
        var row = await db.RsPeriodTimeCostCalcs.AsNoTracking()
            .SingleAsync(x => x.Period == period && x.Project == project && x.Name == "Staff1");
        Assert.Equal(period, row.Period);
        Assert.Equal(project, row.Project);
        Assert.Equal("OPC1", row.OracleProjectCode);
        Assert.Equal("SAC1", row.SubAccountCode);
        Assert.Equal(1, row.Month);
        Assert.Equal("No", row.DefraProject);
        Assert.Equal((double)costCentre, row.Occ ?? 0d);
        Assert.Equal(harness.Id("PC1"), row.Opc);
        Assert.Equal(harness.Id("PC1"), row.Spc);
        Assert.Equal((double)costCentre, row.Scc ?? 0d);
        Assert.Equal("Staff1", row.Name);
        Assert.Equal("GC1", row.GradeCode);
        Assert.Equal(spNumber, row.SpNumber);
        Assert.Equal(10m, row.ChargeRate);
        Assert.Equal(40m, row.Pay);
        Assert.Equal(16m, row.NonPay);
        Assert.Equal(8m, row.Overhead);
        Assert.Equal(8d, row.Time);
        Assert.Equal(80m, row.TotalCost);
    }
}
