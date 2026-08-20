using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Validates that the EF Core model configuration mirrors the SQL schema:
/// column names, required constraints, max-lengths, FK navigation, delete behavior.
/// No DB connection required â€” these assertions run entirely against the in-memory model metadata.
/// Internal entity types are accessed by table-name lookup to avoid visibility constraints.
/// </summary>
public sealed class EfCoreMappingTests
{
    private static readonly DbContextOptions<BatchJobsDbContext> _options =
        new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql("Host=localhost;Database=test") // metadata-only; no connection made
            .Options;

    // Shared instance: metadata-only, no real DB connection is ever opened.
    private readonly BatchJobsDbContext ctx = new BatchJobsDbContext(
        new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql("Host=localhost;Database=test")
            .Options);

    private static IEntityType GetEntityByTable(BatchJobsDbContext ctx, string table) =>
        ctx.Model.GetEntityTypes()
                 .Single(e => e.GetTableName() == table);

    private static IEntityType GetEntityByView(BatchJobsDbContext ctx, string view) =>
        ctx.Model.GetEntityTypes()
                 .Single(e => e.GetViewName() == view);

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // job_lock
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void BatchLock_JobName_MaxLength_Is_255()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = ctx.Model.FindEntityType(typeof(Domain.Entities.BatchLock))!
                       .FindProperty(nameof(Domain.Entities.BatchLock.JobName))!;
        Assert.Equal(255, prop.GetMaxLength());
    }

    [Fact]
    public void BatchLock_JobQueueId_Is_Mapped()
    {
        var prop = ctx.Model.FindEntityType(typeof(Domain.Entities.BatchLock))!
                       .FindProperty(nameof(Domain.Entities.BatchLock.JobQueueId));
        Assert.NotNull(prop);
    }

    [Fact]
    public void BatchLock_TableName_Is_job_lock_In_Fps_Schema()
    {
        var entityType = ctx.Model.FindEntityType(typeof(Domain.Entities.BatchLock))!;
        Assert.Equal("job_lock", entityType.GetTableName());
        Assert.Equal("fps", entityType.GetSchema());
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // job_master
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void TblJobMaster_JobName_MaxLength_Is_100()
    {
        var entityType = GetEntityByTable(ctx, "job_master");
        var prop = entityType.FindProperty("JobName")!;
        Assert.Equal(100, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobMaster_CreatedAt_HasDefaultValueSql()
    {
        var prop = GetEntityByTable(ctx, "job_master").FindProperty("CreatedAt")!;
        Assert.Equal("NOW()", prop.GetDefaultValueSql());
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // job_status FK
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void TblJobStatus_HasForeignKey_To_TblJobMaster_Cascade()
    {
        var master = GetEntityByTable(ctx, "job_master");
        var status = GetEntityByTable(ctx, "job_status");
        var fk = status.GetForeignKeys()
                       .FirstOrDefault(f => f.PrincipalEntityType == master);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // job_queue FK + error message length
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void TblJobQueue_ErrorMessage_MaxLength_Is_1000()
    {
        var prop = GetEntityByTable(ctx, "job_queue").FindProperty("ErrorMessage")!;
        Assert.Equal(1000, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobQueue_RequestedAtUtc_ColumnName_Is_requested_at_utc()
    {
        var prop = GetEntityByTable(ctx, "job_queue").FindProperty("RequestedAtUtc")!;
        Assert.Equal("requested_at_utc", prop.GetColumnName(StoreObjectIdentifier.Table("job_queue", "fps")));
    }

    [Fact]
    public void TblJobQueue_FpsYear_ColumnName_Is_fpsyear()
    {
        var prop = GetEntityByTable(ctx, "job_queue").FindProperty("FpsYear")!;
        Assert.Equal("fpsyear", prop.GetColumnName(StoreObjectIdentifier.Table("job_queue", "fps")));
    }

    [Fact]
    public void TblJobQueue_HasForeignKey_To_TblJobMaster_Restrict()
    {
        var master = GetEntityByTable(ctx, "job_master");
        var queue = GetEntityByTable(ctx, "job_queue");
        var fk = queue.GetForeignKeys()
                      .FirstOrDefault(f => f.PrincipalEntityType == master);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
    }

    [Fact]
    public void TblJobQueue_HasForeignKey_To_TblJobStatus_Restrict()
    {
        var status = GetEntityByTable(ctx, "job_status");
        var queue = GetEntityByTable(ctx, "job_queue");
        var fk = queue.GetForeignKeys()
                      .FirstOrDefault(f => f.PrincipalEntityType == status);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // job_queue_log FK + performed by length
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void TblJobQueueLog_PerformedBy_MaxLength_Is_100()
    {
        var prop = GetEntityByTable(ctx, "job_queue_log").FindProperty("PerformedBy")!;
        Assert.Equal(100, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobQueueLog_Note_MaxLength_Is_500()
    {
        var prop = GetEntityByTable(ctx, "job_queue_log").FindProperty("Note")!;
        Assert.Equal(500, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobQueueLog_HasForeignKey_To_TblJobQueue_Cascade()
    {
        var queue = GetEntityByTable(ctx, "job_queue");
        var log = GetEntityByTable(ctx, "job_queue_log");
        var fk = log.GetForeignKeys()
                    .FirstOrDefault(f => f.PrincipalEntityType == queue);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    [Fact]
    public void TblJobQueueLog_HasForeignKey_To_TblJobStatus_Restrict()
    {
        var status = GetEntityByTable(ctx, "job_status");
        var log = GetEntityByTable(ctx, "job_queue_log");
        var fk = log.GetForeignKeys()
                    .FirstOrDefault(f => f.PrincipalEntityType == status);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // scheduled_load_run
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void ScheduledLoadRun_Table_Exists_In_Fps_Schema()
    {
        var entity = GetEntityByTable(ctx, "scheduled_load_run");
        Assert.Equal("fps", entity.GetSchema());
    }

    [Fact]
    public void ScheduledLoadRun_JobName_MaxLength_Is_100()
    {
        var prop = GetEntityByTable(ctx, "scheduled_load_run").FindProperty("JobName")!;
        Assert.Equal(100, prop.GetMaxLength());
    }

    [Fact]
    public void ScheduledLoadRun_HasForeignKey_To_TblJobMaster_Restrict_Using_JobName()
    {
        var run = GetEntityByTable(ctx, "scheduled_load_run");
        var master = GetEntityByTable(ctx, "job_master");

        var fk = run.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType == master);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
        Assert.Equal("JobName", fk.Properties.Single().Name);
        Assert.Equal("JobName", fk.PrincipalKey.Properties.Single().Name);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // scheduled_load_step_run + scheduled_load_validation_result
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void ScheduledLoadStepRun_HasForeignKey_To_ScheduledLoadRun_Cascade()
    {
        var run = GetEntityByTable(ctx, "scheduled_load_run");
        var step = GetEntityByTable(ctx, "scheduled_load_step_run");
        var fk = step.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType == run);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    [Fact]
    public void ScheduledLoadValidationResult_Has_Unique_Index_On_RunId_And_AssertionCode()
    {
        var validation = GetEntityByTable(ctx, "scheduled_load_validation_result");

        var uniqueIndex = validation
            .GetIndexes()
            .FirstOrDefault(i => i.IsUnique &&
                                 i.Properties.Select(p => p.Name).SequenceEqual(new[] { "RunId", "AssertionCode" }));

        Assert.NotNull(uniqueIndex);
        Assert.Equal("uq_scheduled_load_validation_run_assertion", uniqueIndex!.GetDatabaseName());
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // fps_source_project_year
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void FpsSourceProjectYear_Has_CompositePrimaryKey_Year_ParentProject()
    {
        var source = GetEntityByTable(ctx, "fps_source_project_year");
        Assert.Equal("fps", source.GetSchema());
        var keyProps = source.FindPrimaryKey()!.Properties.Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "Year", "ParentProject" }, keyProps);
    }

    [Fact]
    public void FpsSourceProjectYear_Program_MaxLength_Is_10_And_TotalAdditionalCosts_Is_Money()
    {
        var source = GetEntityByTable(ctx, "fps_source_project_year");

        var program = source.FindProperty("Program")!;
        Assert.Equal(10, program.GetMaxLength());

        var totalAdditional = source.FindProperty("TotalAdditionalCosts")!;
        Assert.Equal("money", totalAdditional.GetColumnType());
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // fps_year_totals + fps_year_archive
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void FpsYearTotals_ProjectStatus_Is_Required_And_CustIncome_Is_Money()
    {
        var totals = GetEntityByTable(ctx, "fps_year_totals");
        Assert.Equal("fps", totals.GetSchema());

        var projectStatus = totals.FindProperty("ProjectStatus")!;
        Assert.False(projectStatus.IsNullable);

        var custIncome = totals.FindProperty("CustIncome")!;
        Assert.Equal("money", custIncome.GetColumnType());
    }

    [Fact]
    public void FpsYearArchive_ArchiveReason_MaxLength_Is_100_And_Has_Default()
    {
        var archive = GetEntityByTable(ctx, "fps_year_archive");
        Assert.Equal("fps", archive.GetSchema());
        var reason = archive.FindProperty("ArchiveReason")!;

        Assert.Equal(100, reason.GetMaxLength());
        Assert.Equal("Before deletion", reason.GetDefaultValue());
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // fps_project_all_current_year
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void FpsProjectAllCurrentYear_Has_CompositePrimaryKey_Year_ParentProject()
    {
        var projectAll = GetEntityByTable(ctx, "fps_project_all_current_year");
        Assert.Equal("fps", projectAll.GetSchema());
        var keyProps = projectAll.FindPrimaryKey()!.Properties.Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "Year", "ParentProject" }, keyProps);
    }

    [Fact]
    public void FpsProjectAllCurrentYear_DateCreated_Uses_Date_Column_Type()
    {
        var projectAll = GetEntityByTable(ctx, "fps_project_all_current_year");
        var dateCreated = projectAll.FindProperty("DateCreated")!;
        Assert.Equal("date", dateCreated.GetColumnType());
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // MilestoneUpdateNotifications read-only sources (mabarchive)
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void VLatestMonthYear_Is_Keyless_View_In_Mabarchive_Schema()
    {
        var entity = GetEntityByView(ctx, "vlatestmonthyear");
        Assert.Equal("mabarchive", entity.GetViewSchema());
        Assert.Null(entity.FindPrimaryKey());
    }

    [Fact]
    public void VProjectReportsPmMilestoneEmail_Is_Keyless_View_With_Expected_Columns()
    {
        var entity = GetEntityByView(ctx, "vprojectreports_pmmilestoneemail");
        Assert.Equal("mabarchive", entity.GetViewSchema());
        Assert.Null(entity.FindPrimaryKey());

        var storeObject = StoreObjectIdentifier.View("vprojectreports_pmmilestoneemail", "mabarchive");
        Assert.Equal("mnumber", entity.FindProperty("MNumber")!.GetColumnName(storeObject));
        Assert.Equal("parentproject", entity.FindProperty("ParentProject")!.GetColumnName(storeObject));
        Assert.Equal("editlink", entity.FindProperty("EditLink")!.GetColumnName(storeObject));
        Assert.Equal("disable", entity.FindProperty("Disable")!.GetColumnName(storeObject));
    }

    [Fact]
    public void TblSettings_Table_Has_Id_PrimaryKey_In_Mabarchive_Schema()
    {
        var entity = GetEntityByTable(ctx, "tbl_settings");
        Assert.Equal("mabarchive", entity.GetSchema());
        var keyProps = entity.FindPrimaryKey()!.Properties.Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "Id" }, keyProps);
    }

    [Fact]
    public void TblCurrentMonth_IsKeyless_Table_In_Fps_Schema()
    {
        // fps.tblcurrentmonth is the authoritative source for latestmonthreleased in TlkpYearLoader.
        var entity = GetEntityByTable(ctx, "tblcurrentmonth");
        Assert.Equal("fps", entity.GetSchema());
        Assert.Null(entity.FindPrimaryKey());
        var storeObject = StoreObjectIdentifier.Table("tblcurrentmonth", "fps");
        Assert.Equal("currentmonth", entity.FindProperty("CurrentMonth")!.GetColumnName(storeObject));
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // single-db / schema guardrails
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public void All_Mapped_Store_Objects_Should_Use_Only_Fps_Or_Mabarchive_Schema()
    {

        var invalid = ctx.Model.GetEntityTypes()
            .Where(e => e.GetTableName() is not null || e.GetViewName() is not null)
            .Select(e => new
            {
                Entity = e.Name,
                Schema = e.GetSchema()
            })
            .Where(x => x.Schema is not null && x.Schema != "fps" && x.Schema != "mabarchive")
            .ToList();

        Assert.True(
            invalid.Count == 0,
            $"Unexpected schema mappings found: {string.Join(", ", invalid.Select(i => $"{i.Entity}:{i.Schema}"))}");
    }

    [Fact]
    public void Mabarchive_Tables_Should_Be_Year_Scoped_Except_Approved_Global_Master_Tables()
    {

        var approvedGlobalMasterTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "g_tlkpproject",
            "tbl_settings" // global config lookup (id-keyed), not year-scoped â€” see MilestoneNotificationsTables.cs
        };

        var invalid = ctx.Model.GetEntityTypes()
            .Where(e => string.Equals(e.GetSchema(), "mabarchive", StringComparison.OrdinalIgnoreCase))
            .Where(e => e.GetTableName() is not null)
            .Where(e => !approvedGlobalMasterTables.Contains(e.GetTableName()!))
            .Where(e => e.FindProperty("Year") is null)
            .Select(e => e.GetTableName()!)
            .OrderBy(n => n)
            .ToList();

        Assert.True(
            invalid.Count == 0,
            $"Mabarchive tables missing required Year column mapping: {string.Join(", ", invalid)}");
    }
}
