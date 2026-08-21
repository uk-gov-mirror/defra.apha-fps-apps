using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

/// <summary>
/// PostgreSQL-backed tests for RecreateSummaries step SQL translation and data-shaping behavior.
/// Skips automatically when a test database is unavailable.
/// </summary>
[Trait("Category", "Integration")]
public sealed class RecreateSummariesPostgresStepIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;

    public RecreateSummariesPostgresStepIntegrationTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = CreateDbContext();

            var requiredTables = new[]
            {
                "projectmonth2",
                "projectmonth3",
                "projectmonthcasework",
                "projectmonthfinal",
                "tlkpproject",
                "period_monthlyoutput",
                "monthlyoutput",
                "workgroup",
                "tlkptestreqmt",
                "costcentre",
                "period_proj_subcontract",
                "proj_subcontract",
                "period_timecostcalcs",
                "timecostcalcs",
                "tblwgemployee"
            };

            var existingTables = await context.Database
                .SqlQueryRaw<string>(@"
                    SELECT table_name
                    FROM information_schema.tables
                    WHERE table_schema = 'fps'")
                .ToListAsync();

            var missingTables = requiredTables
                .Except(existingTables, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (missingTables.Length > 0)
            {
                _skipReason = $"Integration DB missing required fps tables: {string.Join(", ", missingTables)}";
                return;
            }

            await ResetTablesAsync();
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task CreateProjectMonthFinalStep_WhenMonthCutoffApplied_ShouldNullOutFutureCumulativeColumns()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO fps.projectmonth2
                (project, monthno, costprofile, totalcost, invoices, coiw, mstonedue, due__done, ontime, totalhours, paycosts, fpsyear)
            VALUES
                ('PRJ1', 4, 100, 500, 100, 10, 2, 2, 1, 8, 20, 2026),
                ('PRJ1', 8, 150, 900, 200, 20, 3, 3, 2, 12, 30, 2026);

            INSERT INTO fps.projectmonth3
                (project, endperiod, periodname, cumcost, cuminvoices, cumcoiw, cumportsales, cumprofile,
                 sumofcostprofile, sumofmstonedue, sumofdue__done, sumofontime, cumcwdebit, cumcwcredit,
                 cumtotalhours, cumsubcontracts, cumtestcosts, cumpaycosts, fpsyear)
            VALUES
                ('PRJ1', 4, 'P04', 1000, 300, 40, 20, 800, 900, 5, 4, 3, 25, 15, 20, 10, 9, 8, 2026),
                ('PRJ1', 8, 'P08', 2000, 600, 80, 50, 1600, 1700, 8, 7, 6, 55, 35, 40, 20, 18, 16, 2026);

            INSERT INTO fps.projectmonthcasework (project, monthno, fpsyear, cwdebit, cwcredit)
            VALUES
                ('PRJ1', 4, 2026, 11, 9),
                ('PRJ1', 8, 2026, 22, 18);
        ");

        var result = await ExecuteStepAsync("CreateProjectMonthFinalStep", [6], context);

        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage ?? "Expected success status");
        Assert.Equal(2, result.RowsAffected);

        var month4CumFlag = await ScalarNullableIntAsync(
            "SELECT cumflag::int FROM fps.projectmonthfinal WHERE project = 'PRJ1' AND monthno = 4");
        var month4CumCost = await ScalarNullableDecimalAsync(
            "SELECT cumcost FROM fps.projectmonthfinal WHERE project = 'PRJ1' AND monthno = 4");
        var month8CumFlag = await ScalarNullableIntAsync(
            "SELECT cumflag::int FROM fps.projectmonthfinal WHERE project = 'PRJ1' AND monthno = 8");
        var month8CumCost = await ScalarNullableDecimalAsync(
            "SELECT cumcost FROM fps.projectmonthfinal WHERE project = 'PRJ1' AND monthno = 8");
        var month8CwDebit = await ScalarNullableDecimalAsync(
            "SELECT cwdebit FROM fps.projectmonthfinal WHERE project = 'PRJ1' AND monthno = 8");

        Assert.Equal(1, month4CumFlag);
        Assert.NotNull(month4CumCost);
        Assert.Null(month8CumFlag);
        Assert.Null(month8CumCost);
        Assert.Null(month8CwDebit);
    }

    [SkippableFact]
    public async Task RefreshPeriodMoStep_WhenPeriodAlreadyExists_ShouldReplaceRowsForPeriod()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO fps.period_monthlyoutput
                (period, project, isdefraproject, month, spc, testcode, workgroup)
            VALUES
                (6, 'OLD', 'No', 1, 'SPC0', 'OLDT', 'OLDWG');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('PRJMO', 'Project MO', 'PRG1', 'Cust1', 0, 0, 'Active', 'General',
                 1, 10, 'OP-1', 'SA-1', 'IA-1', 2026);

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('WG1', 'SPC1', 20, 2026);

            INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, projectbuyercode, fpsyear)
            VALUES ('T1', 'B1', 3.5, 'PRJMO', 2026);

            INSERT INTO fps.monthlyoutput (buyer, workgroup, testcode, month, volume, fpsyear)
            VALUES ('PRJMO', 'WG1', 'T1', 2, 4, 2026);

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES (10, 'OPC1', 2026);
        ");

        var result = await ExecuteStepAsync("RefreshPeriodMoStep", [6], context);

        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage ?? "Expected success status");

        var rowCount = await ScalarIntAsync("SELECT COUNT(*)::int FROM fps.period_monthlyoutput WHERE period = 6");
        var totalCost = await ScalarNullableDecimalAsync(
            "SELECT totalcost::numeric FROM fps.period_monthlyoutput WHERE period = 6 AND project = 'PRJMO' AND month = 2");
        var defraFlag = await ScalarStringAsync(
            "SELECT isdefraproject FROM fps.period_monthlyoutput WHERE period = 6 AND project = 'PRJMO' AND month = 2");

        Assert.Equal(1, rowCount);
        Assert.Equal(14.0m, totalCost);
        Assert.Equal("Yes", defraFlag);
    }

    [SkippableFact]
    public async Task RefreshPeriodMoStep_WhenVolumeIsNull_ShouldPersistNullTotalCost()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('PRJMONULL', 'Project MO Null', 'PRG1', 'Cust1', 0, 0, 'Active', 'General',
                 1, 10, 'OP-1', 'SA-1', 'IA-1', 2026);

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('WG1', 'SPC1', 20, 2026);

            INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, projectbuyercode, fpsyear)
            VALUES ('T1', 'B1', 3.5, 'PRJMONULL', 2026);

            INSERT INTO fps.monthlyoutput (buyer, workgroup, testcode, month, volume, fpsyear)
            VALUES ('PRJMONULL', 'WG1', 'T1', 2, NULL, 2026);

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES (10, 'OPC1', 2026);
        ");

        var result = await ExecuteStepAsync("RefreshPeriodMoStep", [6], context);

        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage ?? "Expected success status");

        var totalCost = await ScalarNullableDecimalAsync(
            "SELECT totalcost::numeric FROM fps.period_monthlyoutput WHERE period = 6 AND project = 'PRJMONULL' AND month = 2");

        Assert.Null(totalCost);
    }

    [SkippableFact]
    public async Task RefreshPeriodPscStep_WhenPeriodAlreadyExists_ShouldReplaceRowsForPeriod()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO fps.period_proj_subcontract
                (period, subcontcounter, project, isdefraproject, month)
            VALUES
                (6, 999, 'OLD', 'No', 1);

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 isdefraproject, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('PRJPSC', 'Project PSC', 'PRG2', 'Cust2', 0, 0, 'Active', 'General',
                 0, 'OP-2', 'SA-2', 'IA-2', 2026);

            INSERT INTO fps.proj_subcontract (subcontcounter, project, month, amount, acctcode, fpsyear)
            VALUES (100, 'PRJPSC', 3, 50, 'ACCT1', 2026);
        ");

        var result = await ExecuteStepAsync("RefreshPeriodPscStep", [6], context);

        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage ?? "Expected success status");

        var rowCount = await ScalarIntAsync("SELECT COUNT(*) FROM fps.period_proj_subcontract WHERE period = 6");
        var defraFlag = await ScalarStringAsync(
            "SELECT isdefraproject FROM fps.period_proj_subcontract WHERE period = 6 AND subcontcounter = 100");

        Assert.Equal(1, rowCount);
        Assert.Equal("No", defraFlag);
    }

    [SkippableFact]
    public async Task RefreshPeriodTccStep_WhenPeriodAlreadyExists_ShouldReplaceRowsForPeriod()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO fps.period_timecostcalcs
                (period, project, month, defraproject, spc, name, spnumber)
            VALUES
                (6, 'OLD', 1, 'No', 'SPC0', 'OLDUSER', 'OLDSP');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('PRJTCC', 'Project TCC', 'PRG3', 'Cust3', 0, 0, 'Active', 'General',
                 0, 30, 'OP-3', 'SA-3', 'IA-3', 2026);

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES (30, 'OPC30', 2026);

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('WG2', 'SPC2', 40, 2026);

            INSERT INTO fps.tblwgemployee (pactid, spnumber, workgroupgrade, hrspaid, leave, sickspecial, hrsavail, fpsyear)
            VALUES ('S1', 'SPN1', 'WG2', 37, 0, 0, 37, 2026);

            INSERT INTO fps.timecostcalcs
                (project, month, staffid, jobcode, workgroup, name, gradecode, chargerate, pay, nonpay, overhead, time, cost, fpsyear)
            VALUES
                ('PRJTCC', 5, 'S1', 'J1', 'WG2', 'User 1', 'G1', 2.5, 1, 2, 3, 4, 10, 2026);
        ");

        var result = await ExecuteStepAsync("RefreshPeriodTccStep", [6], context);

        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage ?? "Expected success status");

        var rowCount = await ScalarIntAsync("SELECT COUNT(*) FROM fps.period_timecostcalcs WHERE period = 6");
        var totalCost = await ScalarNullableDecimalAsync(
            "SELECT totalcost FROM fps.period_timecostcalcs WHERE period = 6 AND project = 'PRJTCC' AND month = 5 AND name = 'User 1'");
        var spNumber = await ScalarStringAsync(
            "SELECT spnumber FROM fps.period_timecostcalcs WHERE period = 6 AND project = 'PRJTCC' AND month = 5 AND name = 'User 1'");

        Assert.Equal(1, rowCount);
        Assert.Equal(10m, totalCost);
        Assert.Equal("SPN1", spNumber);
    }

    private async Task<StepResult> ExecuteStepAsync(string typeName, object[] args, BatchJobsDbContext context)
    {
        var type = typeof(IRecreateSummariesExecutionStep).Assembly
            .GetType($"Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps.{typeName}");

        Assert.NotNull(type);

        var step = Activator.CreateInstance(type!, args: args) as IRecreateSummariesExecutionStep;
        Assert.NotNull(step);

        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        var executionContext = new RecreateSummariesExecutionContext(context, connection, 2026);

        return await step!.ExecuteAsync(executionContext, CancellationToken.None);
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    private async Task ResetTablesAsync()
    {
        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlRawAsync(@"
            DELETE FROM fps.projectmonthfinal;
            DELETE FROM fps.projectmonthcasework;
            DELETE FROM fps.projectmonth3;
            DELETE FROM fps.projectmonth2;
            DELETE FROM fps.period_monthlyoutput;
            DELETE FROM fps.monthlyoutput;
            DELETE FROM fps.tlkptestreqmt;
            DELETE FROM fps.period_proj_subcontract;
            DELETE FROM fps.proj_subcontract;
            DELETE FROM fps.period_timecostcalcs;
            DELETE FROM fps.timecostcalcs;
            DELETE FROM fps.tblwgemployee;
            DELETE FROM fps.workgroup;
            DELETE FROM fps.costcentre;
            DELETE FROM fps.tlkpproject;
        ");
    }

    private async Task<int> ScalarIntAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return Convert.ToInt32(value);
    }

    private async Task<decimal?> ScalarNullableDecimalAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return value is null || value is DBNull ? null : Convert.ToDecimal(value);
    }

    private async Task<int?> ScalarNullableIntAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return value is null || value is DBNull ? null : Convert.ToInt32(value);
    }

    private async Task<string?> ScalarStringAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return value is null || value is DBNull ? null : value.ToString();
    }
}
