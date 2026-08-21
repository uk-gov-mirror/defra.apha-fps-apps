using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;
using Apha.BatchJobs.Domain.Enums;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class LogRecreateSummariesStepTests
{
    [Fact(Skip = "Requires a live Postgres instance; not run in CI.")]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tblperiod (periodname, endperiod, periodlocked, fpsyear)
            VALUES ('{harness.Id("P5")}', 5, 0, {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), harness.FpsYear);
        var step = new LogRecreateSummariesStep(5, harness.FpsYear, "DOMAIN\\user");
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("LogRecreateSummaries", result.StepName);
        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage);

        // Validate output in RsRecreateSummariesLog
        var row = await db.RsRecreateSummariesLog.AsNoTracking()
            .SingleAsync(x => x.Period == 5 && x.UserId == "user");
        Assert.Equal(5, row.Period);
        Assert.Equal("user", row.UserId); // Normalized
        Assert.True((DateTime.UtcNow - row.DateDone).TotalSeconds < 10); // DateDone is recent
    }

    [Fact(Skip = "Requires a live Postgres instance; not run in CI.")]
    public async Task ExecuteCoreAsync_ResyncsSequenceWhenBehindExistingExplicitIdRows()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tblperiod (periodname, endperiod, periodlocked, fpsyear)
            VALUES ('{harness.Id("P7")}', 7, 0, {harness.FpsYear});
        ");

        // Simulate a prior bulk load that wrote explicit id values, leaving the
        // sequence behind MAX(id) -- the exact drift that caused a live 23505
        // duplicate-key failure against fps.period_timecostcalcs in batchjob_testing.
        var driftedId = await harness.ScalarIntAsync(
            "SELECT COALESCE(MAX(id), 0) + 1000 FROM fps.recreatesummaries_log");
        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.recreatesummaries_log (id, userid, period, datedone, fpsyear)
            VALUES ({driftedId}, 'seed', 1, now(), {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), harness.FpsYear);
        var step = new LogRecreateSummariesStep(7, harness.FpsYear, "DOMAIN\\user2");

        // Act: must not fail with a duplicate-key violation even though the
        // sequence's last_value is now far behind driftedId.
        var result = await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage);
        var row = await db.RsRecreateSummariesLog.AsNoTracking()
            .SingleAsync(x => x.Period == 7 && x.UserId == "user2");
        Assert.True(row.Id > driftedId);
    }
}
