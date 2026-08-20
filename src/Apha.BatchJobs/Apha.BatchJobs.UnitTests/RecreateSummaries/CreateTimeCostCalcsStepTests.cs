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

public sealed class CreateTimeCostCalcsStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;
        var expectedDivision = (await harness.ScalarStringAsync("SELECT divname::text FROM fps.tlkpdivision LIMIT 1")) ?? string.Empty;

        var project = harness.Id("P1");
        var program = $"P{harness.Prefix[2..6]}B";
        var workGroup = harness.Id("WG");
        var jobCode = harness.Id("JC1");
        var pactId = harness.Id("S1");
        var spNumber = harness.Id("SP1");
        var wgGrade = harness.Id("WG1");
        var pcGrade = harness.Id("PCG1");
        var profitCentre = harness.Id("PC1");

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{program}', {harness.FpsYear}, 'Charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, isdefraproject, incomeaccountcode, fpsyear)
            SELECT
                '{project}',
                'Project',
                '{program}',
                p.customer,
                0::money,
                0::money,
                p.projectstatus,
                p.disease,
                p.contract,
                0,
                p.incomeaccountcode,
                {harness.FpsYear}
            FROM fps.tlkpproject p
            WHERE p.fpsyear = {harness.FpsYear}
            LIMIT 1;

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division)
            VALUES ('{profitCentre}', 'Profit Centre', (SELECT divname FROM fps.tlkpdivision LIMIT 1));

            INSERT INTO fps.profitcentregrade
                (pcgrade, divisiongrade, gradecode, profitcentre, chargerate, payrate, npr, ohr, defrachargerate, fpsyear)
            VALUES
                ('{pcGrade}', 'D1', 'GC1', '{profitCentre}', 10::money, 5::money, 2::money, 1::money, 12::money, {harness.FpsYear});

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('{workGroup}', '{profitCentre}', 0, {harness.FpsYear});

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
                ('{pactId}', '{spNumber}', '{wgGrade}', 'A', 'P', 37, 0, 0, 37, 1, 0, {harness.FpsYear});

            INSERT INTO fps.monthlytime
                (pactstaffid, timecode, month, parentproject, workgroup, hours, fpsyear)
            VALUES
                ('{pactId}', '{jobCode}', 1, '{project}', '{workGroup}', 8, {harness.FpsYear});

            INSERT INTO fps.timecodevalid (timecode, workgroup, parentproject, active, fpsyear)
            VALUES ('{jobCode}', '{workGroup}', '{project}', true, {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var deleteStep = new DeleteTimeCostCalcsStep();
        var deleteResult = await deleteStep.ExecuteAsync(context, CancellationToken.None);
        Assert.True(deleteResult.Status == StepStatus.Success, deleteResult.ErrorMessage);

        var step = new CreateTimeCostCalcsStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("CreateTimeCostCalcs", result.StepName);
        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage);

        // Validate output in RsTimeCostCalcs
        var row = await db.RsTimeCostCalcs.AsNoTracking()
            .SingleAsync(x => x.Project == project && x.JobCode == jobCode && x.StaffId == pactId);
        Assert.Equal(workGroup, row.WorkGroup);
        Assert.Equal(jobCode, row.JobCode);
        Assert.Equal(project, row.Project);
        Assert.Equal(1, row.Month);
        Assert.Equal(pactId, row.StaffId);
        Assert.Equal("GC1", row.GradeCode);
        Assert.Equal("One, Staff", row.Name);
        Assert.Equal(10m, row.ChargeRate); // IsDefraProject = 0
        Assert.Equal("Charge", row.Class);
        Assert.Equal(8d, row.Time);
        Assert.Equal(8d * 10d, row.Cost ?? 0d);
        Assert.Equal(expectedDivision, row.Division);
        Assert.Equal(8m * 5m, row.Pay);
        Assert.Equal(8m * 2m, row.NonPay);
        Assert.Equal(8m * 1m, row.Overhead);
        Assert.Equal(harness.FpsYear, row.FpsYear);
    }
}
