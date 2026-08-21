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

public sealed class CreateProjectMonthCaseworkStepTests
{
    [SkippableFact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        var project = harness.Id("PMCW");
        var program = "PRGCW";
        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{program}', {harness.FpsYear}, 'charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, caseworksub, plancaseworkdebit, isdefraproject, incomeaccountcode, fpsyear)
            SELECT
                '{project}',
                'Test Project',
                '{program}',
                p.customer,
                24::money,
                0::money,
                p.projectstatus,
                p.disease,
                p.contract,
                1,
                12::money,
                0,
                p.incomeaccountcode,
                {harness.FpsYear}
            FROM fps.tlkpproject p
            WHERE p.fpsyear = {harness.FpsYear}
            LIMIT 1;

            INSERT INTO fps.projectmonth (project, monthno, costprofile, fpsyear)
            VALUES ('{project}', 1, 0::money, {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
        var deleteStep = new DeleteProjectMonthCaseworkStep();
        var deleteResult = await deleteStep.ExecuteAsync(context, CancellationToken.None);
        Assert.True(deleteResult.Status == StepStatus.Success, deleteResult.ErrorMessage);

        var step = new CreateProjectMonthCaseworkStep();
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("CreateProjectMonthCasework", result.StepName);
        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage);

        var row = await db.RsProjectMonthCasework.AsNoTracking()
            .SingleAsync(x => x.Project == project && x.MonthNo == 1);
        Assert.Equal(project, row.Project);
        Assert.Equal(1, row.MonthNo);
        Assert.Equal(1d, row.CwDebit);
        Assert.Equal(2d, row.CwCredit);
    }
}
