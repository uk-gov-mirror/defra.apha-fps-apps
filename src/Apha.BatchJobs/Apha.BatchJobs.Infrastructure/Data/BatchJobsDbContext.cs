using Apha.BatchJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Data;

/// <summary>
/// Database context for batch jobs fps schema.
/// </summary>
public class BatchJobsDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the BatchJobsDbContext.
    /// </summary>
    /// <param name="options">DbContext configuration options.</param>
    public BatchJobsDbContext(DbContextOptions<BatchJobsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the batch locks table.
    /// </summary>
    public DbSet<BatchLock> BatchLocks { get; set; }

    /// <summary>
    /// Gets or sets the foundation job master table.
    /// </summary>
    internal DbSet<TblJobMaster> TblJobMaster { get; set; }

    /// <summary>
    /// Gets or sets the foundation job status table.
    /// </summary>
    internal DbSet<TblJobStatus> TblJobStatus { get; set; }

    /// <summary>
    /// Gets or sets the foundation job queue table.
    /// </summary>
    internal DbSet<TblJobQueue> TblJobQueue { get; set; }

    /// <summary>
    /// Gets or sets the foundation job queue log table.
    /// </summary>
    internal DbSet<TblJobQueueLog> TblJobQueueLog { get; set; }


    /// <summary>
    /// Gets or sets scheduled load run lifecycle rows.
    /// </summary>
    internal DbSet<ScheduledLoadRunTable> ScheduledLoadRun { get; set; }

    /// <summary>
    /// Gets or sets scheduled load step audit rows.
    /// </summary>
    internal DbSet<ScheduledLoadStepRunTable> ScheduledLoadStepRun { get; set; }

    /// <summary>
    /// Gets or sets scheduled load validation result rows.
    /// </summary>
    internal DbSet<ScheduledLoadValidationResultTable> ScheduledLoadValidationResult { get; set; }

    /// <summary>
    /// Gets or sets source fixture rows for FPS year processing.
    /// </summary>
    internal DbSet<FpsSourceProjectYearTable> FpsSourceProjectYear { get; set; }

    /// <summary>
    /// Gets or sets yearly totals rows.
    /// </summary>
    internal DbSet<FpsYearTotalsTable> FpsYearTotals { get; set; }

    /// <summary>
    /// Gets or sets archived yearly totals rows.
    /// </summary>
    internal DbSet<FpsYearArchiveTable> FpsYearArchive { get; set; }

    /// <summary>
    /// Gets or sets current year project snapshot rows.
    /// </summary>
    internal DbSet<FpsProjectAllCurrentYearTable> FpsProjectAllCurrentYear { get; set; }

    // RecreateSummaries table/view model sets
    internal DbSet<RsFpsYearTotalsTable> RsFpsYearTotals { get; set; }
    internal DbSet<RsTlkpProjectTable> RsTlkpProject { get; set; }
    internal DbSet<RsTlkpProgramTable> RsTlkpProgram { get; set; }
    internal DbSet<RsQryTotalAdditionalCostsView> RsQryTotalAdditionalCosts { get; set; }
    internal DbSet<RsQryTotalAnimalCostsView> RsQryTotalAnimalCosts { get; set; }
    internal DbSet<RsQryTotalStaffCostsView> RsQryTotalStaffCosts { get; set; }
    internal DbSet<RsQryTotalTestCostsView> RsQryTotalTestCosts { get; set; }
    internal DbSet<RsProjectMonthTable> RsProjectMonth { get; set; }
    internal DbSet<RsTimeCostCalcsTable> RsTimeCostCalcs { get; set; }
    internal DbSet<RsProjectMonthCaseworkTable> RsProjectMonthCasework { get; set; }
    internal DbSet<RsQryProjectMonthCwView> RsQryProjectMonthCw { get; set; }
    internal DbSet<RsTblkpProfitCentreTable> RsTblkpProfitCentre { get; set; }
    internal DbSet<RsProfitCentreGradeTable> RsProfitCentreGrade { get; set; }
    internal DbSet<RsWorkGroupGradeTable> RsWorkGroupGrade { get; set; }
    internal DbSet<RsTimeCodeValidTable> RsTimeCodeValid { get; set; }
    internal DbSet<RsVpactTblStaffView> RsVpactTblStaff { get; set; }
    internal DbSet<RsMonthlyTimeTable> RsMonthlyTime { get; set; }
    internal DbSet<RsProjectMonth2Table> RsProjectMonth2 { get; set; }
    internal DbSet<RsProjectMonth3Table> RsProjectMonth3 { get; set; }
    internal DbSet<RsProjectMonthFinalTable> RsProjectMonthFinal { get; set; }
    internal DbSet<RsTblPeriodTable> RsTblPeriod { get; set; }
    internal DbSet<RsTblkPeriodMonthTable> RsTblkPeriodMonth { get; set; }
    internal DbSet<RsQryJobMonthSubContractsView> RsQryJobMonthSubContracts { get; set; }
    internal DbSet<RsQryJobMonthTimeView> RsQryJobMonthTime { get; set; }
    internal DbSet<RsQryJobMonthMilestoneView> RsQryJobMonthMilestone { get; set; }
    internal DbSet<RsQryJobMonthTransfersTotalView> RsQryJobMonthTransfersTotal { get; set; }
    internal DbSet<RsQryJobMonthInvoicesView> RsQryJobMonthInvoices { get; set; }
    internal DbSet<RsQryJobMonthPortfolioSalesView> RsQryJobMonthPortfolioSales { get; set; }
    internal DbSet<RsQryJobMonthTotProfileView> RsQryJobMonthTotProfile { get; set; }
    internal DbSet<RsRecreateSummariesLogTable> RsRecreateSummariesLog { get; set; }
    internal DbSet<RsCostCentreTable> RsCostCentre { get; set; }
    internal DbSet<RsMonthlyOutputTable> RsMonthlyOutput { get; set; }
    internal DbSet<RsWorkGroupTable> RsWorkGroup { get; set; }
    internal DbSet<RsTlkpTestReqmtTable> RsTlkpTestReqmt { get; set; }
    internal DbSet<RsPeriodMonthlyOutputTable> RsPeriodMonthlyOutput { get; set; }
    internal DbSet<RsProjSubContractTable> RsProjSubContract { get; set; }
    internal DbSet<RsPeriodProjSubContractTable> RsPeriodProjSubContract { get; set; }
    internal DbSet<RsTblWgEmployeeTable> RsTblWgEmployee { get; set; }
    internal DbSet<RsPeriodTimeCostCalcsTable> RsPeriodTimeCostCalcs { get; set; }

    // MABArchive LINQ source/target model sets (phase 2 incremental)
    internal DbSet<MaSrcTlkpProgram> MaSrcTlkpProgram { get; set; }
    internal DbSet<MaSrcTlkpProject> MaSrcTlkpProject { get; set; }
    internal DbSet<MaSrcFpsYearTotals> MaSrcFpsYearTotals { get; set; }
    internal DbSet<MaDstMyTlkpProgram> MaDstMyTlkpProgram { get; set; }
    internal DbSet<MaDstGTlkpProject> MaDstGTlkpProject { get; set; }
    internal DbSet<MaDstMyTlkpProject> MaDstMyTlkpProject { get; set; }
    internal DbSet<MaDstMyFpsYearTotals> MaDstMyFpsYearTotals { get; set; }
    internal DbSet<MaSrcMonthlyOutput> MaSrcMonthlyOutput { get; set; }
    internal DbSet<MaDstMyMonthlyOutput> MaDstMyMonthlyOutput { get; set; }
    internal DbSet<MaSrcMonthlyTime> MaSrcMonthlyTime { get; set; }
    internal DbSet<MaDstMyMonthlyTime> MaDstMyMonthlyTime { get; set; }
    internal DbSet<MaSrcProjInvoice> MaSrcProjInvoice { get; set; }
    internal DbSet<MaDstMyProjInvoice> MaDstMyProjInvoice { get; set; }
    internal DbSet<MaSrcProjSubContract> MaSrcProjSubContract { get; set; }
    internal DbSet<MaDstMyProjSubContract> MaDstMyProjSubContract { get; set; }
    internal DbSet<MaSrcProjectMonthFinal> MaSrcProjectMonthFinal { get; set; }
    internal DbSet<MaDstMyProjectMonthFinal> MaDstMyProjectMonthFinal { get; set; }
    internal DbSet<MaSrcTblAdditionalCosts> MaSrcTblAdditionalCosts { get; set; }
    internal DbSet<MaDstMyTblAdditionalCosts> MaDstMyTblAdditionalCosts { get; set; }
    internal DbSet<MaSrcTblAnimalReq> MaSrcTblAnimalReq { get; set; }
    internal DbSet<MaDstMyTblAnimalReq> MaDstMyTblAnimalReq { get; set; }
    internal DbSet<MaSrcTblContract> MaSrcTblContract { get; set; }
    internal DbSet<MaDstMyTblContract> MaDstMyTblContract { get; set; }
    internal DbSet<MaSrcTblStaffJob> MaSrcTblStaffJob { get; set; }
    internal DbSet<MaDstMyTblStaffJob> MaDstMyTblStaffJob { get; set; }
    internal DbSet<MaSrcTimeCostCalcs> MaSrcTimeCostCalcs { get; set; }
    internal DbSet<MaDstMyTimeCostCalcs> MaDstMyTimeCostCalcs { get; set; }
    internal DbSet<MaSrcTlkpTestReqmt> MaSrcTlkpTestReqmt { get; set; }
    internal DbSet<MaDstMyTlkpTestReqmt> MaDstMyTlkpTestReqmt { get; set; }
    internal DbSet<MaSrcTblDbVariable> MaSrcTblDbVariable { get; set; }
    internal DbSet<MaDstTlkpYear> MaDstTlkpYear { get; set; }
    internal DbSet<MaSrcWorkGroupGrade> MaSrcWorkGroupGrade { get; set; }
    internal DbSet<MaDstMyWorkGroupGrade> MaDstMyWorkGroupGrade { get; set; }
    internal DbSet<MaSrcProfitCentreGrade> MaSrcProfitCentreGrade { get; set; }
    internal DbSet<MaDstMyProfitCentreGrade> MaDstMyProfitCentreGrade { get; set; }
    internal DbSet<MaSrcTblkpProfitCentre> MaSrcTblkpProfitCentre { get; set; }
    internal DbSet<MaDstMyTblProfitCentre> MaDstMyTblProfitCentre { get; set; }
    internal DbSet<MaSrcTestOrProduct> MaSrcTestOrProduct { get; set; }
    internal DbSet<MaDstMyTestOrProduct> MaDstMyTestOrProduct { get; set; }
    internal DbSet<MaSrcTblWgEmployee> MaSrcTblWgEmployee { get; set; }
    internal DbSet<MaSrcTblEmployee> MaSrcTblEmployee { get; set; }
    internal DbSet<MaDstMyStaff> MaDstMyStaff { get; set; }
    internal DbSet<MaSrcWorkGroup> MaSrcWorkGroup { get; set; }
    internal DbSet<MaDstMyWorkGroup> MaDstMyWorkGroup { get; set; }
    internal DbSet<MaSrcTblUsers> MaSrcTblUsers { get; set; }
    internal DbSet<MaSrcTblUserProfitCentre> MaSrcTblUserProfitCentre { get; set; }
    internal DbSet<MaSrcTblAnimals> MaSrcTblAnimals { get; set; }
    internal DbSet<MaDstMyTblAnimals> MaDstMyTblAnimals { get; set; }
    internal DbSet<MaDstMyTlkpProjectAll> MaDstMyTlkpProjectAll { get; set; }

    /// <summary>
    /// Configures the model for the database context.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure BatchLock table — mirrors fps.job_lock
        modelBuilder.Entity<BatchLock>(entity =>
        {
            entity.ToTable("job_lock", schema: "fps");
            entity.HasKey(e => e.LockId);
            entity.Property(e => e.LockId).HasColumnName("lock_id").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobName).HasColumnName("job_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.AcquiredAt).HasColumnName("acquired_at").IsRequired();
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
            entity.HasIndex(e => e.JobName).HasDatabaseName("idx_job_lock_job_name");
            entity.HasIndex(e => new { e.JobName, e.IsActive }).HasDatabaseName("idx_job_lock_job_name_active");
            entity.HasIndex(e => e.JobName).IsUnique().HasDatabaseName("uq_job_lock_job_name_active").HasFilter("is_active = true");
            entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("idx_job_lock_expires_at");
        });

        // Configure foundation job master table — mirrors fps.job_master
        modelBuilder.Entity<TblJobMaster>(entity =>
        {
            entity.ToTable("job_master", schema: "fps");
            entity.HasKey(e => e.JobId);
            entity.Property(e => e.JobId).HasColumnName("jobid").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobName).HasColumnName("jobname").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Frequency).HasColumnName("frequency").HasMaxLength(50);
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(250);
            entity.Property(e => e.TimeToLive).HasColumnName("timetolive").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.JobName).IsUnique().HasDatabaseName("job_master_jobname_key");
        });

        // Configure foundation job status table — mirrors fps.job_status
        modelBuilder.Entity<TblJobStatus>(entity =>
        {
            entity.ToTable("job_status", schema: "fps");
            entity.HasKey(e => e.StatusId);
            entity.Property(e => e.StatusId).HasColumnName("statusid").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobId).HasColumnName("jobid").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.JobId, e.Status }).IsUnique().HasDatabaseName("uq_job_status_jobid_status");
            entity.HasOne<TblJobMaster>()
                  .WithMany()
                  .HasForeignKey(e => e.JobId)
                  .HasConstraintName("fk_job_status_jobid")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure foundation job queue table — mirrors fps.job_queue
        modelBuilder.Entity<TblJobQueue>(entity =>
        {
            entity.ToTable("job_queue", schema: "fps");
            entity.HasKey(e => e.JobQueueId);
            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.JobExecutionId).HasColumnName("jobexecutionid").IsRequired();
            entity.Property(e => e.JobId).HasColumnName("jobid").IsRequired();
            entity.Property(e => e.StatusId).HasColumnName("statusid").IsRequired();
            entity.Property(e => e.RequestedBy).HasColumnName("requestedby").IsRequired().HasMaxLength(100);
            entity.Property(e => e.RequestedAtUtc).HasColumnName("requested_at_utc");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.StartDateTime).HasColumnName("startdatetime");
            entity.Property(e => e.EndDateTime).HasColumnName("enddatetime");
            entity.Property(e => e.ErrorMessage).HasColumnName("errormessage").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.JobExecutionId).IsUnique().HasDatabaseName("uq_job_queue_jobexecutionid");
            entity.HasIndex(e => e.RequestedBy).HasDatabaseName("idx_job_queue_requestedby");
            entity.HasOne<TblJobMaster>()
                  .WithMany()
                  .HasForeignKey(e => e.JobId)
                  .HasConstraintName("fk_job_queue_jobid")
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TblJobStatus>()
                  .WithMany()
                  .HasForeignKey(e => e.StatusId)
                  .HasConstraintName("fk_job_queue_statusid")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure foundation job queue log table — mirrors fps.job_queue_log
        modelBuilder.Entity<TblJobQueueLog>(entity =>
        {
            entity.ToTable("job_queue_log", schema: "fps");
            entity.HasKey(e => e.JobQueueLogId);
            entity.Property(e => e.JobQueueLogId).HasColumnName("jobqueuelogid").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid").IsRequired();
            entity.Property(e => e.StatusId).HasColumnName("statusid").IsRequired();
            entity.Property(e => e.PerformedBy).HasColumnName("performedby").IsRequired().HasMaxLength(100);
            entity.Property(e => e.LogTime).HasColumnName("logtime").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(500);
            entity.HasOne<TblJobQueue>()
                  .WithMany()
                  .HasForeignKey(e => e.JobQueueId)
                  .HasConstraintName("fk_job_queue_log_jobqueueid")
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<TblJobStatus>()
                  .WithMany()
                  .HasForeignKey(e => e.StatusId)
                  .HasConstraintName("fk_job_queue_log_statusid")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure scheduled load run table — mirrors fps.scheduled_load_run
        modelBuilder.Entity<ScheduledLoadRunTable>(entity =>
        {
            entity.ToTable("scheduled_load_run", schema: "fps");
            entity.HasKey(e => e.RunId).HasName("pk_scheduled_load_run");
            entity.Property(e => e.RunId).HasColumnName("run_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.JobName).HasColumnName("job_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.FpsYear).HasColumnName("fps_year").IsRequired();
            entity.Property(e => e.JobStartedAt).HasColumnName("job_started_at").IsRequired();
            entity.Property(e => e.JobCompletedAt).HasColumnName("job_completed_at");
            entity.Property(e => e.FinalStatus).HasColumnName("final_status").HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasColumnName("correlation_id").IsRequired().HasMaxLength(64);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");

            entity.HasIndex(e => new { e.JobName, e.FpsYear }).HasDatabaseName("idx_scheduled_load_run_job_fps_year");
            entity.HasIndex(e => e.CorrelationId).HasDatabaseName("idx_scheduled_load_run_correlation_id");

            entity.HasOne<TblJobMaster>()
                  .WithMany()
                  .HasForeignKey(e => e.JobName)
                  .HasPrincipalKey(e => e.JobName)
                  .HasConstraintName("fk_scheduled_load_run_jobname")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure scheduled load step run table — mirrors fps.scheduled_load_step_run
        modelBuilder.Entity<ScheduledLoadStepRunTable>(entity =>
        {
            entity.ToTable("scheduled_load_step_run", schema: "fps");
            entity.HasKey(e => e.StepRunId).HasName("pk_scheduled_load_step_run");
            entity.Property(e => e.StepRunId).HasColumnName("step_run_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.RunId).HasColumnName("run_id").IsRequired();
            entity.Property(e => e.StepName).HasColumnName("step_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.StepSequence).HasColumnName("step_sequence").IsRequired();
            entity.Property(e => e.StartedAt).HasColumnName("started_at").IsRequired();
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.StepStatus).HasColumnName("step_status").IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(500);
            entity.Property(e => e.RowsAffected).HasColumnName("rows_affected");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.RunId).HasDatabaseName("idx_scheduled_load_step_run_run_id");
            entity.HasIndex(e => e.StepStatus).HasDatabaseName("idx_scheduled_load_step_run_status");

            entity.HasOne<ScheduledLoadRunTable>()
                  .WithMany()
                  .HasForeignKey(e => e.RunId)
                  .HasConstraintName("fk_scheduled_load_step_run_run_id")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure scheduled load validation result table — mirrors fps.scheduled_load_validation_result
        modelBuilder.Entity<ScheduledLoadValidationResultTable>(entity =>
        {
            entity.ToTable("scheduled_load_validation_result", schema: "fps");
            entity.HasKey(e => e.ValidationId).HasName("pk_scheduled_load_validation_result");
            entity.Property(e => e.ValidationId).HasColumnName("validation_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.RunId).HasColumnName("run_id").IsRequired();
            entity.Property(e => e.AssertionCode).HasColumnName("assertion_code").IsRequired().HasMaxLength(50);
            entity.Property(e => e.AssertionDescription).HasColumnName("assertion_description").IsRequired().HasMaxLength(500);
            entity.Property(e => e.ExpectedValue).HasColumnName("expected_value").HasColumnType("numeric(18,2)");
            entity.Property(e => e.ActualValue).HasColumnName("actual_value").HasColumnType("numeric(18,2)");
            entity.Property(e => e.Passed).HasColumnName("passed").IsRequired();
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(500);
            entity.Property(e => e.CheckedAt).HasColumnName("checked_at").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");

            entity.HasIndex(e => new { e.RunId, e.Passed }).HasDatabaseName("idx_scheduled_load_validation_run_passed");
            entity.HasIndex(e => e.AssertionCode).HasDatabaseName("idx_scheduled_load_validation_assertion_code");
            entity.HasIndex(e => new { e.RunId, e.AssertionCode })
                  .IsUnique()
                  .HasDatabaseName("uq_scheduled_load_validation_run_assertion");

            entity.HasOne<ScheduledLoadRunTable>()
                  .WithMany()
                  .HasForeignKey(e => e.RunId)
                  .HasConstraintName("fk_scheduled_load_validation_result_run_id")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure fps source fixture table — fps.fps_source_project_year
        modelBuilder.Entity<FpsSourceProjectYearTable>(entity =>
        {
            entity.ToTable("fps_source_project_year", schema: "fps");
            entity.HasKey(e => new { e.Year, e.ParentProject }).HasName("pk_fps_source_project_year");
            entity.Property(e => e.Year).HasColumnName("year").HasColumnType("smallint").IsRequired();
            entity.Property(e => e.ParentProject).HasColumnName("parentproject").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Program).HasColumnName("program").IsRequired().HasMaxLength(10);
            entity.Property(e => e.TotalAdditionalCosts).HasColumnName("totaladditionalcosts").HasColumnType("money");
            entity.Property(e => e.TotalAnimalCosts).HasColumnName("totalanimalcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalStaffCosts).HasColumnName("totalstaffcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalTestCosts).HasColumnName("totaltestcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalCosts).HasColumnName("totalcosts").HasColumnType("double precision");
            entity.Property(e => e.CustIncome).HasColumnName("custincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TotalIncome).HasColumnName("totalincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl").HasColumnType("money");
            entity.Property(e => e.RequiredProfit).HasColumnName("requiredprofit").HasColumnType("money");
            entity.Property(e => e.Manager).HasColumnName("manager").HasMaxLength(50);
            entity.Property(e => e.Customer).HasColumnName("customer").HasMaxLength(50);
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus").HasMaxLength(50);
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome").HasColumnType("money");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit").HasColumnType("money");
            entity.Property(e => e.TotalPayCosts).HasColumnName("totalpaycosts").HasColumnType("double precision");

            entity.HasIndex(e => e.Year).HasDatabaseName("idx_fps_source_project_year_fps_year");
        });

        // Configure fps year totals table — fps.fps_year_totals
        modelBuilder.Entity<FpsYearTotalsTable>(entity =>
        {
            entity.ToTable("fps_year_totals", schema: "fps");
            entity.HasKey(e => new { e.Year, e.ParentProject }).HasName("pk_fps_year_totals");
            entity.Property(e => e.Year).HasColumnName("year").HasColumnType("smallint").IsRequired();
            entity.Property(e => e.ParentProject).HasColumnName("parentproject").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Program).HasColumnName("program").IsRequired().HasMaxLength(10);
            entity.Property(e => e.TotalAdditionalCosts).HasColumnName("totaladditionalcosts").HasColumnType("money");
            entity.Property(e => e.TotalAnimalCosts).HasColumnName("totalanimalcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalStaffCosts).HasColumnName("totalstaffcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalTestCosts).HasColumnName("totaltestcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalCosts).HasColumnName("totalcosts").HasColumnType("double precision");
            entity.Property(e => e.CustIncome).HasColumnName("custincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TotalIncome).HasColumnName("totalincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl").HasColumnType("money");
            entity.Property(e => e.RequiredProfit).HasColumnName("requiredprofit").HasColumnType("money");
            entity.Property(e => e.Manager).HasColumnName("manager").HasMaxLength(50);
            entity.Property(e => e.Customer).HasColumnName("customer").HasMaxLength(50);
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus").IsRequired().HasMaxLength(50);
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome").HasColumnType("money");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit").HasColumnType("money");
            entity.Property(e => e.TotalPayCosts).HasColumnName("totalpaycosts").HasColumnType("double precision");

            entity.HasIndex(e => e.Year).HasDatabaseName("idx_fps_year_totals_fps_year");
        });

        // Configure fps year archive table — fps.fps_year_archive
        modelBuilder.Entity<FpsYearArchiveTable>(entity =>
        {
            entity.ToTable("fps_year_archive", schema: "fps");
            entity.HasKey(e => new { e.Year, e.ParentProject }).HasName("pk_fps_year_archive");
            entity.Property(e => e.Year).HasColumnName("year").HasColumnType("smallint").IsRequired();
            entity.Property(e => e.ParentProject).HasColumnName("parentproject").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Program).HasColumnName("program").IsRequired().HasMaxLength(10);
            entity.Property(e => e.TotalAdditionalCosts).HasColumnName("totaladditionalcosts").HasColumnType("money");
            entity.Property(e => e.TotalAnimalCosts).HasColumnName("totalanimalcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalStaffCosts).HasColumnName("totalstaffcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalTestCosts).HasColumnName("totaltestcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalCosts).HasColumnName("totalcosts").HasColumnType("double precision");
            entity.Property(e => e.CustIncome).HasColumnName("custincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TotalIncome).HasColumnName("totalincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl").HasColumnType("money");
            entity.Property(e => e.RequiredProfit).HasColumnName("requiredprofit").HasColumnType("money");
            entity.Property(e => e.Manager).HasColumnName("manager").HasMaxLength(50);
            entity.Property(e => e.Customer).HasColumnName("customer").HasMaxLength(50);
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus").IsRequired().HasMaxLength(50);
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome").HasColumnType("money");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit").HasColumnType("money");
            entity.Property(e => e.TotalPayCosts).HasColumnName("totalpaycosts").HasColumnType("double precision");
            entity.Property(e => e.ArchivedAt).HasColumnName("archived_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.ArchiveReason).HasColumnName("archive_reason").IsRequired().HasMaxLength(100).HasDefaultValue("Before deletion");

            entity.HasIndex(e => e.Year).HasDatabaseName("idx_fps_year_archive_fps_year");
            entity.HasIndex(e => e.ArchivedAt).HasDatabaseName("idx_fps_year_archive_archived_at");
        });

        // Configure fps current year project-all table — fps.fps_project_all_current_year
        modelBuilder.Entity<FpsProjectAllCurrentYearTable>(entity =>
        {
            entity.ToTable("fps_project_all_current_year", schema: "fps");
            entity.HasKey(e => new { e.Year, e.ParentProject }).HasName("pk_fps_project_all_current_year");
            entity.Property(e => e.Year).HasColumnName("year").HasColumnType("smallint").IsRequired();
            entity.Property(e => e.ParentProject).HasColumnName("parentproject").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Program).HasColumnName("program").HasMaxLength(10);
            entity.Property(e => e.Customer).HasColumnName("customer").HasMaxLength(50);
            entity.Property(e => e.Manager).HasColumnName("manager").HasMaxLength(50);
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome").HasColumnType("money");
            entity.Property(e => e.CustIncome).HasColumnName("custincome").HasColumnType("money");
            entity.Property(e => e.WipEoy).HasColumnName("wip_eoy").HasColumnType("money");
            entity.Property(e => e.WipLimit).HasColumnName("wip_limit").HasColumnType("money");
            entity.Property(e => e.WipCurrent).HasColumnName("wip_current").HasColumnType("money");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus").HasMaxLength(50);
            entity.Property(e => e.DateCreated).HasColumnName("datecreated").HasColumnType("date");
            entity.Property(e => e.FecCost).HasColumnName("feccost").HasColumnType("money");
            entity.Property(e => e.Profit).HasColumnName("profit").HasColumnType("money");
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl").HasColumnType("money");
            entity.Property(e => e.CaseworkSub).HasColumnName("caseworksub").HasColumnType("numeric(5,4)");
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome").HasColumnType("money");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit").HasColumnType("money");
            entity.Property(e => e.Source).HasColumnName("source").HasColumnType("character(5)");
            entity.Property(e => e.Disease).HasColumnName("disease").HasMaxLength(50);
            entity.Property(e => e.Contract).HasColumnName("contract").HasMaxLength(10);
            entity.Property(e => e.Finished).HasColumnName("finished").HasColumnType("smallint");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.CarryOver).HasColumnName("carryover").HasColumnType("money");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject").HasColumnType("smallint");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre").HasColumnType("double precision");
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode").HasMaxLength(50);
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode").HasMaxLength(50);
            entity.Property(e => e.ProjectGroup).HasColumnName("projectgroup").HasMaxLength(50);
            entity.Property(e => e.IncomeAccountCode).HasColumnName("incomeaccountcode").HasMaxLength(50);
            entity.Property(e => e.RefreshedAt).HasColumnName("refreshed_at").IsRequired().HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Year).HasDatabaseName("idx_fps_project_all_current_year_fps_year");
        });

        ConfigureRecreateSummariesModels(modelBuilder);
        ConfigureMabArchiveModels(modelBuilder);
    }

    private static void ConfigureMabArchiveModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaSrcTlkpProgram>(entity =>
        {
            entity.ToView("tlkpprogram", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.ProgramNo });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.ProgramNo).HasColumnName("programno");
            entity.Property(e => e.ProgramName).HasColumnName("programname");
            entity.Property(e => e.Directorate).HasColumnName("directorate");
            entity.Property(e => e.Minim).HasColumnName("minim");
            entity.Property(e => e.SectorName).HasColumnName("sector_name");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Target).HasColumnName("target");
            entity.Property(e => e.Manager).HasColumnName("manager");
        });

        modelBuilder.Entity<MaSrcTlkpProject>(entity =>
        {
            entity.ToView("tlkpproject", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.ParentProject });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.ProjectTitle).HasColumnName("projecttitle");
            entity.Property(e => e.CostBookNo).HasColumnName("costbookno");
            entity.Property(e => e.Disease).HasColumnName("disease");
            entity.Property(e => e.Contract).HasColumnName("contract");
            entity.Property(e => e.ShortTitle).HasColumnName("shorttitle");
            entity.Property(e => e.Program).HasColumnName("program");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome");
            entity.Property(e => e.CustIncome).HasColumnName("custincome");
            entity.Property(e => e.WipEoy).HasColumnName("wip_eoy");
            entity.Property(e => e.WipLimit).HasColumnName("wip_limit");
            entity.Property(e => e.WipCurrent).HasColumnName("wip_current");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus");
            entity.Property(e => e.DateCreated).HasColumnName("datecreated").HasColumnType("timestamp without time zone");
            entity.Property(e => e.FecCost).HasColumnName("feccost");
            entity.Property(e => e.Profit).HasColumnName("profit");
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl");
            entity.Property(e => e.CaseworkSub).HasColumnName("caseworksub");
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.CarryOver).HasColumnName("carryover");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode");
            entity.Property(e => e.ProjectGroup).HasColumnName("projectgroup");
            entity.Property(e => e.IncomeAccountCode).HasColumnName("incomeaccountcode");
        });

        modelBuilder.Entity<MaSrcFpsYearTotals>(entity =>
        {
            entity.ToView("fpsyeartotals", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.ParentProject });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.Program).HasColumnName("program");
            entity.Property(e => e.TotalAdditionalCosts).HasColumnName("totaladditionalcosts");
            entity.Property(e => e.TotalAnimalCosts).HasColumnName("totalanimalcosts");
            entity.Property(e => e.TotalStaffCosts).HasColumnName("totalstaffcosts");
            entity.Property(e => e.TotalTestCosts).HasColumnName("totaltestcosts");
            entity.Property(e => e.TotalCosts).HasColumnName("totalcosts");
            entity.Property(e => e.CustIncome).HasColumnName("custincome");
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome");
            entity.Property(e => e.TotalIncome).HasColumnName("totalincome");
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl");
            entity.Property(e => e.RequiredProfit).HasColumnName("requiredprofit");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus");
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit");
            entity.Property(e => e.TotalPayCosts).HasColumnName("totalpaycosts");
        });

        modelBuilder.Entity<MaDstMyTlkpProgram>(entity =>
        {
            entity.ToTable("my_tlkpprogram", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.ProgramNo });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ProgramNo).HasColumnName("programno");
            entity.Property(e => e.ProgramName).HasColumnName("programname");
            entity.Property(e => e.Directorate).HasColumnName("directorate");
            entity.Property(e => e.Minim).HasColumnName("minim");
            entity.Property(e => e.SectorName).HasColumnName("sector_name");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Target).HasColumnName("target");
            entity.Property(e => e.Manager).HasColumnName("manager");
        });

        modelBuilder.Entity<MaDstGTlkpProject>(entity =>
        {
            entity.ToTable("g_tlkpproject", schema: "mabarchive");
            entity.HasKey(e => e.ParentProject);
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.ProjectTitle).HasColumnName("projecttitle");
            entity.Property(e => e.CostBookNo).HasColumnName("costbookno");
            entity.Property(e => e.Disease).HasColumnName("disease");
            entity.Property(e => e.Contract).HasColumnName("contract");
            entity.Property(e => e.ShortTitle).HasColumnName("shorttitle");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus");
        });

        modelBuilder.Entity<MaDstMyTlkpProject>(entity =>
        {
            entity.ToTable("my_tlkpproject", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.ParentProject });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.Program).HasColumnName("program");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome");
            entity.Property(e => e.CustIncome).HasColumnName("custincome");
            entity.Property(e => e.WipEoy).HasColumnName("wip_eoy");
            entity.Property(e => e.WipLimit).HasColumnName("wip_limit");
            entity.Property(e => e.WipCurrent).HasColumnName("wip_current");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus");
            entity.Property(e => e.DateCreated).HasColumnName("datecreated").HasColumnType("timestamp without time zone");
            entity.Property(e => e.FecCost).HasColumnName("feccost");
            entity.Property(e => e.Profit).HasColumnName("profit");
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl");
            entity.Property(e => e.CaseworkSub).HasColumnName("caseworksub");
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit");
            entity.Property(e => e.Disease).HasColumnName("disease");
            entity.Property(e => e.Contract).HasColumnName("contract");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.CarryOver).HasColumnName("carryover");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode");
            entity.Property(e => e.ProjectGroup).HasColumnName("projectgroup");
            entity.Property(e => e.IncomeAccountCode).HasColumnName("incomeaccountcode");
        });

        modelBuilder.Entity<MaDstMyFpsYearTotals>(entity =>
        {
            entity.ToTable("my_fpsyeartotals", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.ParentProject });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.Program).HasColumnName("program");
            entity.Property(e => e.TotalAdditionalCosts).HasColumnName("totaladditionalcosts");
            entity.Property(e => e.TotalAnimalCosts).HasColumnName("totalanimalcosts");
            entity.Property(e => e.TotalStaffCosts).HasColumnName("totalstaffcosts");
            entity.Property(e => e.TotalTestCosts).HasColumnName("totaltestcosts");
            entity.Property(e => e.TotalCosts).HasColumnName("totalcosts");
            entity.Property(e => e.CustIncome).HasColumnName("custincome");
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome");
            entity.Property(e => e.TotalIncome).HasColumnName("totalincome");
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl");
            entity.Property(e => e.RequiredProfit).HasColumnName("requiredprofit");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus");
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit");
            entity.Property(e => e.TotalPayCosts).HasColumnName("totalpaycosts");
        });

        modelBuilder.Entity<MaSrcMonthlyOutput>(entity =>
        {
            entity.ToView("monthlyoutput", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.TestCode, e.Buyer, e.Month, e.WorkGroup });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.TestCode).HasColumnName("testcode");
            entity.Property(e => e.Buyer).HasColumnName("buyer");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.WgBuyer).HasColumnName("wgbuyer");
        });

        modelBuilder.Entity<MaDstMyMonthlyOutput>(entity =>
        {
            entity.ToTable("my_monthlyoutput", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.TestCode, e.Buyer, e.Month, e.WorkGroup });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.TestCode).HasColumnName("testcode");
            entity.Property(e => e.Buyer).HasColumnName("buyer");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.WgBuyer).HasColumnName("wgbuyer");
        });

        modelBuilder.Entity<MaSrcMonthlyTime>(entity =>
        {
            entity.ToView("monthlytime", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.PactStaffId, e.TimeCode, e.Month, e.ParentProject, e.WorkGroup });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.PactStaffId).HasColumnName("pactstaffid");
            entity.Property(e => e.TimeCode).HasColumnName("timecode");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.Hours).HasColumnName("hours");
        });

        modelBuilder.Entity<MaDstMyMonthlyTime>(entity =>
        {
            entity.ToTable("my_monthlytime", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.PactStaffId, e.TimeCode, e.Month, e.ParentProject, e.WorkGroup });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.PactStaffId).HasColumnName("pactstaffid");
            entity.Property(e => e.TimeCode).HasColumnName("timecode");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.Hours).HasColumnName("hours");
        });

        modelBuilder.Entity<MaSrcProjInvoice>(entity =>
        {
            entity.ToView("proj_invoice", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.ProjectParent).HasColumnName("projectparent");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CostOfWork).HasColumnName("costofwork");
            entity.Property(e => e.Wip).HasColumnName("wip");
            entity.Property(e => e.ProfitLoss).HasColumnName("profitloss");
            entity.Property(e => e.Detail).HasColumnName("detail");
            entity.Property(e => e.InvoiceCounter).HasColumnName("invoicecounter");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<MaDstMyProjInvoice>(entity =>
        {
            entity.ToTable("my_proj_invoice", schema: "mabarchive");
            // Real DB primary key is (year, projectparent, invoicecounter) -- see
            // dbscript/schemas/02mabarchive/01tables/my_proj_invoice.sql (pk_my_proj_invoice).
            // Month is NOT part of the key and IS nullable there; it was incorrectly included in the EF
            // key previously, which forced Month non-nullable in this entity and caused the loader to
            // silently drop legacy rows with a null month (CR-028).
            entity.HasKey(e => new { e.Year, e.ProjectParent, e.InvoiceCounter });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ProjectParent).HasColumnName("projectparent");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CostOfWork).HasColumnName("costofwork");
            entity.Property(e => e.Wip).HasColumnName("wip");
            entity.Property(e => e.ProfitLoss).HasColumnName("profitloss");
            entity.Property(e => e.Detail).HasColumnName("detail");
            entity.Property(e => e.InvoiceCounter).HasColumnName("invoicecounter");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<MaSrcProjSubContract>(entity =>
        {
            entity.ToView("proj_subcontract", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.SubContCounter });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.SubContCounter).HasColumnName("subcontcounter");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.TestJob).HasColumnName("testjob");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.AcctCode).HasColumnName("acctcode");
            entity.Property(e => e.Supplier).HasColumnName("supplier");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.SupplierNumber).HasColumnName("suppliernumber");
            entity.Property(e => e.DailyRate).HasColumnName("dailyrate");
            entity.Property(e => e.AnimalDays).HasColumnName("animaldays");
        });

        modelBuilder.Entity<MaDstMyProjSubContract>(entity =>
        {
            entity.ToTable("my_proj_subcontract", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.SubContCounter });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.SubContCounter).HasColumnName("subcontcounter");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.TestJob).HasColumnName("testjob");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.AcctCode).HasColumnName("acctcode");
            entity.Property(e => e.Supplier).HasColumnName("supplier");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.SupplierNumber).HasColumnName("suppliernumber");
            entity.Property(e => e.DailyRate).HasColumnName("dailyrate");
            entity.Property(e => e.AnimalDays).HasColumnName("animaldays");
        });

        modelBuilder.Entity<MaSrcProjectMonthFinal>(entity =>
        {
            entity.ToView("projectmonthfinal", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.Project, e.MonthNo, e.PeriodName });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.PeriodName).HasColumnName("periodname");
            entity.Property(e => e.CumFlag).HasColumnName("cumflag");
            entity.Property(e => e.CostProfile).HasColumnName("costprofile");
            entity.Property(e => e.SubContracts).HasColumnName("subcontracts");
            entity.Property(e => e.Animals).HasColumnName("animals");
            entity.Property(e => e.NonAnimals).HasColumnName("nonanimals");
            entity.Property(e => e.TimeCosts).HasColumnName("timecosts");
            entity.Property(e => e.TransferCosts).HasColumnName("transfercosts");
            entity.Property(e => e.TotalCost).HasColumnName("totalcost");
            entity.Property(e => e.Invoices).HasColumnName("invoices");
            entity.Property(e => e.Coiw).HasColumnName("coiw");
            entity.Property(e => e.PortSales).HasColumnName("portsales");
            entity.Property(e => e.CumCost).HasColumnName("cumcost");
            entity.Property(e => e.CumProfile).HasColumnName("cumprofile");
            entity.Property(e => e.SumOfCostProfile).HasColumnName("sumofcostprofile");
            entity.Property(e => e.CumInvoices).HasColumnName("cuminvoices");
            entity.Property(e => e.CumCoiw).HasColumnName("cumcoiw");
            entity.Property(e => e.CumPortSales).HasColumnName("cumportsales");
            entity.Property(e => e.MstoneDue).HasColumnName("mstonedue");
            entity.Property(e => e.DueDone).HasColumnName("due__done");
            entity.Property(e => e.OnTime).HasColumnName("ontime");
            entity.Property(e => e.SumOfMstoneDue).HasColumnName("sumofmstonedue");
            entity.Property(e => e.SumOfDueDone).HasColumnName("sumofdue__done");
            entity.Property(e => e.SumOfOnTime).HasColumnName("sumofontime");
            entity.Property(e => e.CwDebit).HasColumnName("cwdebit");
            entity.Property(e => e.CwCredit).HasColumnName("cwcredit");
            entity.Property(e => e.CumCwDebit).HasColumnName("cumcwdebit");
            entity.Property(e => e.CumCwCredit).HasColumnName("cumcwcredit");
            entity.Property(e => e.TotalHours).HasColumnName("totalhours");
            entity.Property(e => e.CumTotalHours).HasColumnName("cumtotalhours");
            entity.Property(e => e.CumSubContracts).HasColumnName("cumsubcontracts");
            entity.Property(e => e.CumTestCosts).HasColumnName("cumtestcosts");
            entity.Property(e => e.PayCosts).HasColumnName("paycosts");
            entity.Property(e => e.CumPayCosts).HasColumnName("cumpaycosts");
        });

        modelBuilder.Entity<MaDstMyProjectMonthFinal>(entity =>
        {
            entity.ToTable("my_projectmonthfinal", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.Project, e.MonthNo, e.PeriodName });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.PeriodName).HasColumnName("periodname");
            entity.Property(e => e.CumFlag).HasColumnName("cumflag");
            entity.Property(e => e.CostProfile).HasColumnName("costprofile");
            entity.Property(e => e.SubContracts).HasColumnName("subcontracts");
            entity.Property(e => e.Animals).HasColumnName("animals");
            entity.Property(e => e.NonAnimals).HasColumnName("nonanimals");
            entity.Property(e => e.TimeCosts).HasColumnName("timecosts");
            entity.Property(e => e.TransferCosts).HasColumnName("transfercosts");
            entity.Property(e => e.TotalCost).HasColumnName("totalcost");
            entity.Property(e => e.Invoices).HasColumnName("invoices");
            entity.Property(e => e.Coiw).HasColumnName("coiw");
            entity.Property(e => e.PortSales).HasColumnName("portsales");
            entity.Property(e => e.CumCost).HasColumnName("cumcost");
            entity.Property(e => e.CumProfile).HasColumnName("cumprofile");
            entity.Property(e => e.SumOfCostProfile).HasColumnName("sumofcostprofile");
            entity.Property(e => e.CumInvoices).HasColumnName("cuminvoices");
            entity.Property(e => e.CumCoiw).HasColumnName("cumcoiw");
            entity.Property(e => e.CumPortSales).HasColumnName("cumportsales");
            entity.Property(e => e.MstoneDue).HasColumnName("mstonedue");
            entity.Property(e => e.DueDone).HasColumnName("due__done");
            entity.Property(e => e.OnTime).HasColumnName("ontime");
            entity.Property(e => e.SumOfMstoneDue).HasColumnName("sumofmstonedue");
            entity.Property(e => e.SumOfDueDone).HasColumnName("sumofdue__done");
            entity.Property(e => e.SumOfOnTime).HasColumnName("sumofontime");
            entity.Property(e => e.CwDebit).HasColumnName("cwdebit");
            entity.Property(e => e.CwCredit).HasColumnName("cwcredit");
            entity.Property(e => e.CumCwDebit).HasColumnName("cumcwdebit");
            entity.Property(e => e.CumCwCredit).HasColumnName("cumcwcredit");
            entity.Property(e => e.TotalHours).HasColumnName("totalhours");
            entity.Property(e => e.CumTotalHours).HasColumnName("cumtotalhours");
            entity.Property(e => e.CumSubContracts).HasColumnName("cumsubcontracts");
            entity.Property(e => e.CumTestCosts).HasColumnName("cumtestcosts");
            entity.Property(e => e.PayCosts).HasColumnName("paycosts");
            entity.Property(e => e.CumPayCosts).HasColumnName("cumpaycosts");
        });

        modelBuilder.Entity<MaSrcTblAdditionalCosts>(entity =>
        {
            entity.ToView("tbladditionalcosts", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.JobCode, e.Account, e.Description });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.Account).HasColumnName("account");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ItemCost).HasColumnName("itemcost");
            entity.Property(e => e.Freq).HasColumnName("freq");
            entity.Property(e => e.Supplier).HasColumnName("supplier");
        });

        modelBuilder.Entity<MaDstMyTblAdditionalCosts>(entity =>
        {
            entity.ToTable("my_tbladditionalcosts", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.JobCode, e.Account, e.Description });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.Account).HasColumnName("account");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ItemCost).HasColumnName("itemcost");
            entity.Property(e => e.Freq).HasColumnName("freq");
            entity.Property(e => e.Supplier).HasColumnName("supplier");
            entity.Property(e => e.AcCounter).HasColumnName("ac_counter");
        });

        modelBuilder.Entity<MaSrcTblAnimalReq>(entity =>
        {
            entity.ToView("tblanimalreq", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.IndCounter });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.AnimalType).HasColumnName("animaltype");
            entity.Property(e => e.NumberOfDays).HasColumnName("numberofdays");
            entity.Property(e => e.NumberOfAnimals).HasColumnName("numberofanimals");
            entity.Property(e => e.IndCounter).HasColumnName("indcounter");
        });

        modelBuilder.Entity<MaDstMyTblAnimalReq>(entity =>
        {
            entity.ToTable("my_tblanimalreq", schema: "mabarchive");
            entity.HasKey(e => e.ArCounter);
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.AnimalType).HasColumnName("animaltype");
            entity.Property(e => e.NumberOfDays).HasColumnName("numberofdays");
            entity.Property(e => e.NumberOfAnimals).HasColumnName("numberofanimals");
            entity.Property(e => e.ArCounter).HasColumnName("ar_counter").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<MaSrcTblContract>(entity =>
        {
            entity.ToView("tblcontract", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.ContractNo });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.ContractNo).HasColumnName("contractno");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.RegisteredDate).HasColumnName("registereddate");
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.EndDate).HasColumnName("enddate");
            entity.Property(e => e.ContractDoc).HasColumnName("contractdoc");
            entity.Property(e => e.Duration).HasColumnName("duration");
        });

        modelBuilder.Entity<MaDstMyTblContract>(entity =>
        {
            entity.ToTable("my_tblcontract", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.ContractNo });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ContractNo).HasColumnName("contractno");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.RegisteredDate).HasColumnName("registereddate");
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.EndDate).HasColumnName("enddate");
            entity.Property(e => e.ContractDoc).HasColumnName("contractdoc");
            entity.Property(e => e.Duration).HasColumnName("duration");
        });

        modelBuilder.Entity<MaSrcTblStaffJob>(entity =>
        {
            entity.ToView("tblstaffjob", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.StaffId, e.JobCode });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.StaffId).HasColumnName("staffid");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.PlannedHours).HasColumnName("plannedhours");
        });

        modelBuilder.Entity<MaDstMyTblStaffJob>(entity =>
        {
            entity.ToTable("my_tblstaffjob", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.StaffId, e.JobCode });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.StaffId).HasColumnName("staffid");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.PlannedHours).HasColumnName("plannedhours");
        });

        modelBuilder.Entity<MaSrcTimeCostCalcs>(entity =>
        {
            entity.ToView("timecostcalcs", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.Project, e.Month, e.StaffId, e.JobCode });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.StaffId).HasColumnName("staffid");
            entity.Property(e => e.GradeCode).HasColumnName("gradecode");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ChargeRate).HasColumnName("chargerate");
            entity.Property(e => e.Class).HasColumnName("class");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Division).HasColumnName("division");
            entity.Property(e => e.JobCodeOld).HasColumnName("jobcodeold");
            entity.Property(e => e.Pay).HasColumnName("pay");
            entity.Property(e => e.NonPay).HasColumnName("nonpay");
            entity.Property(e => e.Overhead).HasColumnName("overhead");
        });

        modelBuilder.Entity<MaDstMyTimeCostCalcs>(entity =>
        {
            entity.ToTable("my_timecostcalcs", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.Project, e.Month, e.StaffId, e.JobCode });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.StaffId).HasColumnName("staffid");
            entity.Property(e => e.GradeCode).HasColumnName("gradecode");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ChargeRate).HasColumnName("chargerate");
            entity.Property(e => e.Class).HasColumnName("class");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Division).HasColumnName("division");
            entity.Property(e => e.JobCodeOld).HasColumnName("jobcodeold");
            entity.Property(e => e.Pay).HasColumnName("pay");
            entity.Property(e => e.NonPay).HasColumnName("nonpay");
            entity.Property(e => e.Overhead).HasColumnName("overhead");
        });

        modelBuilder.Entity<MaSrcTlkpTestReqmt>(entity =>
        {
            entity.ToView("tlkptestreqmt", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.ProjectBuyerCode, e.TestCode });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.TestCode).HasColumnName("testcode");
            entity.Property(e => e.Buyer).HasColumnName("buyer");
            entity.Property(e => e.UnitPrice).HasColumnName("unitprice");
            entity.Property(e => e.NoRequired).HasColumnName("norequired");
            entity.Property(e => e.ProjectBuyerCode).HasColumnName("projectbuyercode");
            entity.Property(e => e.TestBuyerCode).HasColumnName("testbuyercode");
        });

        modelBuilder.Entity<MaDstMyTlkpTestReqmt>(entity =>
        {
            entity.ToTable("my_tlkptestreqmt", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.ProjectBuyerCode, e.TestCode });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.TestCode).HasColumnName("testcode");
            entity.Property(e => e.Buyer).HasColumnName("buyer");
            entity.Property(e => e.UnitPrice).HasColumnName("unitprice");
            entity.Property(e => e.NoRequired).HasColumnName("norequired");
            entity.Property(e => e.ProjectBuyerCode).HasColumnName("projectbuyercode");
            entity.Property(e => e.TestBuyerCode).HasColumnName("testbuyercode");
        });

        modelBuilder.Entity<MaSrcTblDbVariable>(entity =>
        {
            entity.ToView("tbldb_variables", schema: "fps");
            entity.HasKey(e => e.DbVarName);
            entity.Property(e => e.DbVarName).HasColumnName("db_var_name");
            entity.Property(e => e.DbVarValue).HasColumnName("db_var_value");
        });

        modelBuilder.Entity<MaDstTlkpYear>(entity =>
        {
            entity.ToTable("tlkpyear", schema: "mabarchive");
            entity.HasKey(e => e.Year);
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.LatestMonthReleased).HasColumnName("latestmonthreleased");
        });

        modelBuilder.Entity<MaSrcWorkGroupGrade>(entity =>
        {
            entity.ToView("workgroupgrade", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.WgGrade });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.WgGrade).HasColumnName("wggrade");
            entity.Property(e => e.ProfitCentreGrade).HasColumnName("profitcentregrade");
            entity.Property(e => e.GradeCode).HasColumnName("gradecode");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
        });

        modelBuilder.Entity<MaDstMyWorkGroupGrade>(entity =>
        {
            entity.ToTable("my_workgroupgrade", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.WgGrade });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.WgGrade).HasColumnName("wggrade");
            entity.Property(e => e.ProfitCentreGrade).HasColumnName("profitcentregrade");
            entity.Property(e => e.GradeCode).HasColumnName("gradecode");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
        });

        modelBuilder.Entity<MaSrcProfitCentreGrade>(entity =>
        {
            entity.ToView("profitcentregrade", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.PcGrade });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.PcGrade).HasColumnName("pcgrade");
            entity.Property(e => e.DivisionGrade).HasColumnName("divisiongrade");
            entity.Property(e => e.GradeCode).HasColumnName("gradecode");
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.ChargeRate).HasColumnName("chargerate");
            entity.Property(e => e.DirectRate).HasColumnName("directrate");
            entity.Property(e => e.PayRate).HasColumnName("payrate");
            entity.Property(e => e.Npr).HasColumnName("npr");
            entity.Property(e => e.Ohr).HasColumnName("ohr");
        });

        modelBuilder.Entity<MaDstMyProfitCentreGrade>(entity =>
        {
            entity.ToTable("my_profitcentregrade", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.PcGrade });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.PcGrade).HasColumnName("pcgrade");
            entity.Property(e => e.DivisionGrade).HasColumnName("divisiongrade");
            entity.Property(e => e.GradeCode).HasColumnName("gradecode");
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.ChargeRate).HasColumnName("chargerate");
            entity.Property(e => e.DirectRate).HasColumnName("directrate");
            entity.Property(e => e.PayRate).HasColumnName("payrate");
            entity.Property(e => e.Npr).HasColumnName("npr");
            entity.Property(e => e.Ohr).HasColumnName("ohr");
        });

        modelBuilder.Entity<MaSrcTblkpProfitCentre>(entity =>
        {
            entity.ToView("tblkpprofitcentre", schema: "fps");
            entity.HasKey(e => e.ProfitCentre);
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.ProfitCentreName).HasColumnName("profitcentrename");
            entity.Property(e => e.Division).HasColumnName("division");
            entity.Property(e => e.ContTarget).HasColumnName("conttarget");
            entity.Property(e => e.ProfitCentreHead).HasColumnName("profitcentrehead");
            entity.Property(e => e.DivisionId).HasColumnName("divisionid");
        });

        modelBuilder.Entity<MaDstMyTblProfitCentre>(entity =>
        {
            entity.ToTable("my_tblprofitcentre", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.ProfitCentre });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.ProfitCentreName).HasColumnName("profitcentrename");
            entity.Property(e => e.Division).HasColumnName("division");
            entity.Property(e => e.ContTarget).HasColumnName("conttarget");
            entity.Property(e => e.ProfitCentreHead).HasColumnName("profitcentrehead");
            entity.Property(e => e.DivisionId).HasColumnName("divisionid");
        });

        modelBuilder.Entity<MaSrcTestOrProduct>(entity =>
        {
            entity.ToView("testorproduct", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.ItemCode });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.ItemCode).HasColumnName("itemcode");
            entity.Property(e => e.ItemDescription).HasColumnName("itemdescription");
            entity.Property(e => e.TestManager).HasColumnName("testmanager");
            entity.Property(e => e.JobStatus).HasColumnName("jobstatus");
            entity.Property(e => e.UnitPriceVla).HasColumnName("unitpricevla");
            entity.Property(e => e.PriceAhvg).HasColumnName("priceahvg");
            entity.Property(e => e.Owner).HasColumnName("owner");
            entity.Property(e => e.ChargeMethod).HasColumnName("chargemethod");
            entity.Property(e => e.ShortDescription).HasColumnName("shortdescription");
            entity.Property(e => e.DefraUnitPrice).HasColumnName("defraunitprice");
        });

        modelBuilder.Entity<MaDstMyTestOrProduct>(entity =>
        {
            entity.ToTable("my_testorproduct", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.ItemCode });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ItemCode).HasColumnName("itemcode");
            entity.Property(e => e.ItemDescription).HasColumnName("itemdescription");
            entity.Property(e => e.TestManager).HasColumnName("testmanager");
            entity.Property(e => e.JobStatus).HasColumnName("jobstatus");
            entity.Property(e => e.UnitPriceVla).HasColumnName("unitpricevla");
            entity.Property(e => e.PriceAhvg).HasColumnName("priceahvg");
            entity.Property(e => e.Owner).HasColumnName("owner");
            entity.Property(e => e.ChargeMethod).HasColumnName("chargemethod");
            entity.Property(e => e.ShortDescription).HasColumnName("shortdescription");
            entity.Property(e => e.DefraUnitPrice).HasColumnName("defraunitprice");
        });

        modelBuilder.Entity<MaSrcTblWgEmployee>(entity =>
        {
            entity.ToView("tblwgemployee", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.PactId });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.PactId).HasColumnName("pactid");
            entity.Property(e => e.SpNumber).HasColumnName("spnumber");
            entity.Property(e => e.WorkGroupGrade).HasColumnName("workgroupgrade");
            entity.Property(e => e.PersonStatus).HasColumnName("personstatus");
            entity.Property(e => e.PersonClass).HasColumnName("personclass");
            entity.Property(e => e.HrsPaid).HasColumnName("hrspaid");
            entity.Property(e => e.LeaveHours).HasColumnName("leave");
            entity.Property(e => e.SickSpecial).HasColumnName("sickspecial");
            entity.Property(e => e.HrsAvail).HasColumnName("hrsavail");
        });

        modelBuilder.Entity<MaSrcTblEmployee>(entity =>
        {
            entity.ToView("tblemployee", schema: "fps");
            entity.HasKey(e => e.SpNumber);
            entity.Property(e => e.SpNumber).HasColumnName("spnumber");
            entity.Property(e => e.LastName).HasColumnName("lastname");
            entity.Property(e => e.FirstName).HasColumnName("firstname");
            entity.Property(e => e.Title).HasColumnName("title");
        });

        modelBuilder.Entity<MaDstMyStaff>(entity =>
        {
            entity.ToTable("my_staff", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.StaffId });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.StaffId).HasColumnName("staffid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.WorkGroupGrade).HasColumnName("workgroupgrade");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.PersonStatus).HasColumnName("personstatus");
            entity.Property(e => e.PersonClass).HasColumnName("personclass");
            entity.Property(e => e.HrsPaid).HasColumnName("hrspaid");
            entity.Property(e => e.LeaveHours).HasColumnName("leave");
            entity.Property(e => e.SickSpecial).HasColumnName("sickspecial");
            entity.Property(e => e.HrsAvail).HasColumnName("hrsavail");
        });

        modelBuilder.Entity<MaSrcWorkGroup>(entity =>
        {
            entity.ToView("workgroup", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.WorkGroup });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.Owner).HasColumnName("owner");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CentralOverhead).HasColumnName("centraloverhead");
            entity.Property(e => e.SendEmail).HasColumnName("sendemail");
            entity.Property(e => e.Cos90).HasColumnName("cos90");
            entity.Property(e => e.CostCentreOld).HasColumnName("costcentreold");
            entity.Property(e => e.EmailRecipient).HasColumnName("email_recipient");
        });

        modelBuilder.Entity<MaDstMyWorkGroup>(entity =>
        {
            entity.ToTable("my_workgroup", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.WorkGroup });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.Owner).HasColumnName("owner");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CentralOverhead).HasColumnName("centraloverhead");
            entity.Property(e => e.SendEmail).HasColumnName("sendemail");
            entity.Property(e => e.Cos90).HasColumnName("cos90");
            entity.Property(e => e.CostCentreOld).HasColumnName("costcentreold");
            entity.Property(e => e.EmailRecipient).HasColumnName("email_recipient");
        });

        // CR-028: supports the ported sp_AddMY_Staff ProfitCentre authorization filter (see MyStaffLoader).
        modelBuilder.Entity<MaSrcTblUsers>(entity =>
        {
            entity.ToView("tblusers", schema: "fps");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserName).HasColumnName("username");
        });

        // CR-028: supports the ported sp_AddMY_Staff ProfitCentre authorization filter (see MyStaffLoader).
        modelBuilder.Entity<MaSrcTblUserProfitCentre>(entity =>
        {
            entity.ToView("tbluser_profitcentre", schema: "fps");
            entity.HasKey(e => new { e.ProfitCentre, e.UserId });
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        modelBuilder.Entity<MaSrcTblAnimals>(entity =>
        {
            entity.ToView("tblanimals", schema: "fps");
            entity.HasKey(e => new { e.FpsYear, e.AnimalType });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.AnimalType).HasColumnName("animaltype");
            entity.Property(e => e.Species).HasColumnName("species");
            entity.Property(e => e.SecurityLevel).HasColumnName("security_level");
            entity.Property(e => e.DailyRate).HasColumnName("dailyrate");
            entity.Property(e => e.PlanByWeek).HasColumnName("planbyweek");
            entity.Property(e => e.DefraDailyRate).HasColumnName("defradailyrate");
        });

        modelBuilder.Entity<MaDstMyTblAnimals>(entity =>
        {
            entity.ToTable("my_tblanimals", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.AnimalType });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.AnimalType).HasColumnName("animaltype");
            entity.Property(e => e.Species).HasColumnName("species");
            entity.Property(e => e.SecurityLevel).HasColumnName("security_level");
            entity.Property(e => e.DailyRate).HasColumnName("dailyrate");
            entity.Property(e => e.PlanByWeek).HasColumnName("planbyweek");
            entity.Property(e => e.DefraDailyRate).HasColumnName("defradailyrate");
        });

        modelBuilder.Entity<MaDstMyTlkpProjectAll>(entity =>
        {
            entity.ToTable("my_tlkpproject_all", schema: "mabarchive");
            entity.HasKey(e => new { e.Year, e.ParentProject });
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.Program).HasColumnName("program");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome");
            entity.Property(e => e.CustIncome).HasColumnName("custincome");
            entity.Property(e => e.WipEoy).HasColumnName("wip_eoy");
            entity.Property(e => e.WipLimit).HasColumnName("wip_limit");
            entity.Property(e => e.WipCurrent).HasColumnName("wip_current");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus");
            entity.Property(e => e.DateCreated).HasColumnName("datecreated");
            entity.Property(e => e.FecCost).HasColumnName("feccost");
            entity.Property(e => e.Profit).HasColumnName("profit");
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl");
            entity.Property(e => e.CaseworkSub).HasColumnName("caseworksub");
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit");
            entity.Property(e => e.Disease).HasColumnName("disease");
            entity.Property(e => e.Contract).HasColumnName("contract");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.CarryOver).HasColumnName("carryover");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode");
            entity.Property(e => e.ProjectGroup).HasColumnName("projectgroup");
            entity.Property(e => e.IncomeAccountCode).HasColumnName("incomeaccountcode");
        });
    }

    private static void ConfigureRecreateSummariesModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RsFpsYearTotalsTable>(entity =>
        {
            entity.ToTable("fpsyeartotals", schema: "fps");
            entity.HasKey(e => new { e.ParentProject, e.FpsYear });
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.Program).HasColumnName("program");
            entity.Property(e => e.TotalAdditionalCosts).HasColumnName("totaladditionalcosts");
            entity.Property(e => e.TotalAnimalCosts).HasColumnName("totalanimalcosts");
            entity.Property(e => e.TotalStaffCosts).HasColumnName("totalstaffcosts");
            entity.Property(e => e.TotalTestCosts).HasColumnName("totaltestcosts");
            entity.Property(e => e.TotalCosts).HasColumnName("totalcosts");
            entity.Property(e => e.CustIncome).HasColumnName("custincome");
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome");
            entity.Property(e => e.TotalIncome).HasColumnName("totalincome");
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl");
            entity.Property(e => e.RequiredProfit).HasColumnName("requiredprofit");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus");
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit");
            entity.Property(e => e.TotalPayCosts).HasColumnName("totalpaycosts");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        modelBuilder.Entity<RsTlkpProjectTable>(entity =>
        {
            entity.ToTable("tlkpproject", schema: "fps");
            entity.HasKey(e => new { e.ParentProject, e.FpsYear });
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.Program).HasColumnName("program");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit");
            entity.Property(e => e.CustIncome).HasColumnName("custincome");
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome");
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl");
            entity.Property(e => e.Profit).HasColumnName("profit");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus");
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome");
            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear")
                .HasConversion<double>();
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.IsDefraProject)
                .HasColumnName("isdefraproject")
                .HasConversion<double?>();
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode");
        });

        modelBuilder.Entity<RsTlkpProgramTable>(entity =>
        {
            entity.ToTable("tlkpprogram", schema: "fps");
            entity.HasKey(e => e.ProgramNo);
            entity.Property(e => e.ProgramNo).HasColumnName("programno");
            entity.Property(e => e.SectorName).HasColumnName("sector_name");
        });

        modelBuilder.Entity<RsProjectMonthTable>(entity =>
        {
            entity.ToTable("projectmonth", schema: "fps");
            entity.HasKey(e => new { e.Project, e.MonthNo, e.FpsYear });
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.CostProfile).HasColumnName("costprofile");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        modelBuilder.Entity<RsTimeCostCalcsTable>(entity =>
        {
            entity.ToTable("timecostcalcs", schema: "fps");
            entity.HasKey(e => new { e.WorkGroup, e.JobCode, e.Project, e.Month, e.StaffId, e.FpsYear });
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.Month).HasColumnName("month").HasConversion<double>();
            entity.Property(e => e.StaffId).HasColumnName("staffid");
            entity.Property(e => e.GradeCode).HasColumnName("gradecode");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ChargeRate).HasColumnName("chargerate").HasColumnType("money");
            entity.Property(e => e.Class).HasColumnName("class");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Division).HasColumnName("division");
            entity.Property(e => e.Pay).HasColumnName("pay").HasColumnType("money");
            entity.Property(e => e.NonPay).HasColumnName("nonpay").HasColumnType("money");
            entity.Property(e => e.Overhead).HasColumnName("overhead").HasColumnType("money");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        modelBuilder.Entity<RsProjectMonthCaseworkTable>(entity =>
        {
            entity.ToTable("projectmonthcasework", schema: "fps");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.HasKey(e => new { e.Project, e.MonthNo, e.FpsYear });
            entity.Property(e => e.CwDebit).HasColumnName("cwdebit");
            entity.Property(e => e.CwCredit).HasColumnName("cwcredit");
        });

        modelBuilder.Entity<RsProjectMonth2Table>(entity =>
        {
            entity.ToTable("projectmonth2", schema: "fps");
            entity.HasKey(e => new { e.Project, e.MonthNo, e.FpsYear });
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno").HasConversion<double>();
            entity.Property(e => e.CostProfile).HasColumnName("costprofile").HasColumnType("money");
            entity.Property(e => e.SubContracts).HasColumnName("subcontracts").HasColumnType("money");
            entity.Property(e => e.Animals).HasColumnName("animals").HasColumnType("money");
            entity.Property(e => e.NonAnimal).HasColumnName("nonanimal").HasColumnType("money");
            entity.Property(e => e.TimeCosts).HasColumnName("timecosts");
            entity.Property(e => e.TransferCosts).HasColumnName("transfercosts");
            entity.Property(e => e.TotalCost).HasColumnName("totalcost").HasColumnType("money");
            entity.Property(e => e.Invoices).HasColumnName("invoices").HasColumnType("money");
            entity.Property(e => e.Coiw).HasColumnName("coiw").HasColumnType("money");
            entity.Property(e => e.SumOfCostProfile).HasColumnName("sumofcostprofile").HasColumnType("money");
            entity.Property(e => e.PortSales).HasColumnName("portsales");
            entity.Property(e => e.MstoneDue).HasColumnName("mstonedue").HasConversion<int?>();
            entity.Property(e => e.DueDone).HasColumnName("due__done");
            entity.Property(e => e.OnTime).HasColumnName("ontime");
            entity.Property(e => e.TotalHours).HasColumnName("totalhours");
            entity.Property(e => e.PayCosts).HasColumnName("paycosts");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        modelBuilder.Entity<RsProjectMonth3Table>(entity =>
        {
            entity.ToTable("projectmonth3", schema: "fps");
            entity.HasKey(e => new { e.Project, e.EndPeriod, e.FpsYear });
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.EndPeriod).HasColumnName("endperiod").HasConversion<double>();
            entity.Property(e => e.PeriodName).HasColumnName("periodname");
            entity.Property(e => e.CumCost).HasColumnName("cumcost").HasColumnType("money");
            entity.Property(e => e.CumInvoices).HasColumnName("cuminvoices").HasColumnType("money");
            entity.Property(e => e.CumCoiw).HasColumnName("cumcoiw").HasColumnType("money");
            entity.Property(e => e.CumPortSales).HasColumnName("cumportsales").HasConversion<double?>();
            entity.Property(e => e.CumProfile).HasColumnName("cumprofile").HasColumnType("money");
            entity.Property(e => e.SumOfCostProfile).HasColumnName("sumofcostprofile").HasColumnType("money");
            entity.Property(e => e.SumOfMstoneDue).HasColumnName("sumofmstonedue");
            entity.Property(e => e.SumOfDueDone).HasColumnName("sumofdue__done");
            entity.Property(e => e.SumOfOnTime).HasColumnName("sumofontime");
            entity.Property(e => e.CumCwDebit).HasColumnName("cumcwdebit").HasColumnType("money");
            entity.Property(e => e.CumCwCredit).HasColumnName("cumcwcredit").HasColumnType("money");
            entity.Property(e => e.CumTotalHours).HasColumnName("cumtotalhours");
            entity.Property(e => e.CumSubContracts).HasColumnName("cumsubcontracts");
            entity.Property(e => e.CumTestCosts).HasColumnName("cumtestcosts");
            entity.Property(e => e.CumPayCosts).HasColumnName("cumpaycosts");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        modelBuilder.Entity<RsProjectMonthFinalTable>(entity =>
        {
            entity.ToTable("projectmonthfinal", schema: "fps");
            entity.HasKey(e => new { e.Project, e.MonthNo, e.FpsYear });
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno").HasConversion<double>();
            entity.Property(e => e.CostProfile).HasColumnName("costprofile").HasColumnType("money");
            entity.Property(e => e.SubContracts).HasColumnName("subcontracts").HasColumnType("money");
            entity.Property(e => e.Animals).HasColumnName("animals").HasColumnType("money");
            entity.Property(e => e.NonAnimals).HasColumnName("nonanimals").HasColumnType("money");
            entity.Property(e => e.TimeCosts).HasColumnName("timecosts").HasColumnType("money");
            entity.Property(e => e.TransferCosts).HasColumnName("transfercosts").HasColumnType("money");
            entity.Property(e => e.TotalCost).HasColumnName("totalcost").HasColumnType("money");
            entity.Property(e => e.Invoices).HasColumnName("invoices").HasColumnType("money");
            entity.Property(e => e.Coiw).HasColumnName("coiw").HasColumnType("money");
            entity.Property(e => e.PortSales).HasColumnName("portsales").HasColumnType("money");
            entity.Property(e => e.CumCost).HasColumnName("cumcost").HasColumnType("money");
            entity.Property(e => e.CumProfile).HasColumnName("cumprofile").HasColumnType("money");
            entity.Property(e => e.PeriodName).HasColumnName("periodname");
            entity.Property(e => e.SumOfCostProfile).HasColumnName("sumofcostprofile").HasColumnType("money");
            entity.Property(e => e.CumInvoices).HasColumnName("cuminvoices").HasColumnType("money");
            entity.Property(e => e.CumCoiw).HasColumnName("cumcoiw").HasColumnType("money");
            entity.Property(e => e.CumPortSales).HasColumnName("cumportsales").HasColumnType("money");
            entity.Property(e => e.MstoneDue).HasColumnName("mstonedue").HasConversion<int?>();
            entity.Property(e => e.DueDone).HasColumnName("due__done");
            entity.Property(e => e.OnTime).HasColumnName("ontime");
            entity.Property(e => e.SumOfMstoneDue).HasColumnName("sumofmstonedue");
            entity.Property(e => e.SumOfDueDone).HasColumnName("sumofdue__done");
            entity.Property(e => e.SumOfOnTime).HasColumnName("sumofontime");
            entity.Property(e => e.CumFlag).HasColumnName("cumflag").HasConversion<double?>();
            entity.Property(e => e.CwDebit).HasColumnName("cwdebit").HasColumnType("money");
            entity.Property(e => e.CwCredit).HasColumnName("cwcredit").HasColumnType("money");
            entity.Property(e => e.CumCwDebit).HasColumnName("cumcwdebit").HasColumnType("money");
            entity.Property(e => e.CumCwCredit).HasColumnName("cumcwcredit").HasColumnType("money");
            entity.Property(e => e.TotalHours).HasColumnName("totalhours");
            entity.Property(e => e.CumTotalHours).HasColumnName("cumtotalhours");
            entity.Property(e => e.CumSubContracts).HasColumnName("cumsubcontracts");
            entity.Property(e => e.CumTestCosts).HasColumnName("cumtestcosts");
            entity.Property(e => e.PayCosts).HasColumnName("paycosts");
            entity.Property(e => e.CumPayCosts).HasColumnName("cumpaycosts");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        modelBuilder.Entity<RsTblPeriodTable>(entity =>
        {
            entity.ToTable("tblperiod", schema: "fps");
            entity.HasKey(e => e.EndPeriod);
            entity.Property(e => e.EndPeriod).HasColumnName("endperiod").HasConversion<double>();
            entity.Property(e => e.PeriodName).HasColumnName("periodname");
            entity.Property(e => e.PeriodLocked).HasColumnName("periodlocked");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        modelBuilder.Entity<RsTblkPeriodMonthTable>(entity =>
        {
            entity.ToTable("tblkperiodmonth", schema: "fps");
            entity.HasKey(e => new { e.PeriodName, e.MonthNo });
            entity.Property(e => e.PeriodName).HasColumnName("periodname");
            entity.Property(e => e.MonthNo).HasColumnName("monthno").HasConversion<double>();
        });

        modelBuilder.Entity<RsTblkpProfitCentreTable>(entity =>
        {
            entity.ToTable("tblkpprofitcentre", schema: "fps");
            entity.HasKey(e => e.ProfitCentre);
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.Division).HasColumnName("division");
        });

        modelBuilder.Entity<RsProfitCentreGradeTable>(entity =>
        {
            entity.ToTable("profitcentregrade", schema: "fps");
            entity.HasKey(e => e.PcGrade);
            entity.Property(e => e.PcGrade).HasColumnName("pcgrade");
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.ChargeRate).HasColumnName("chargerate");
            entity.Property(e => e.DefraChargeRate).HasColumnName("defrachargerate");
            entity.Property(e => e.PayRate).HasColumnName("payrate");
            entity.Property(e => e.Npr).HasColumnName("npr");
            entity.Property(e => e.Ohr).HasColumnName("ohr");
        });

        modelBuilder.Entity<RsWorkGroupGradeTable>(entity =>
        {
            entity.ToTable("workgroupgrade", schema: "fps");
            entity.HasKey(e => e.WgGrade);
            entity.Property(e => e.WgGrade).HasColumnName("wggrade");
            entity.Property(e => e.ProfitCentreGrade).HasColumnName("profitcentregrade");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.GradeCode).HasColumnName("gradecode");
        });

        modelBuilder.Entity<RsTimeCodeValidTable>(entity =>
        {
            entity.ToTable("timecodevalid", schema: "fps");
            entity.HasKey(e => new { e.WorkGroup, e.TimeCode, e.ParentProject });
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.TimeCode).HasColumnName("timecode");
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
        });

        modelBuilder.Entity<RsMonthlyTimeTable>(entity =>
        {
            entity.ToTable("monthlytime", schema: "fps");
            entity.HasKey(e => new { e.PactStaffId, e.WorkGroup, e.TimeCode, e.ParentProject, e.Month });
            entity.Property(e => e.PactStaffId).HasColumnName("pactstaffid");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.TimeCode).HasColumnName("timecode");
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.Month)
                .HasColumnName("month")
                .HasConversion<double>();
            entity.Property(e => e.Hours).HasColumnName("hours");
        });

        modelBuilder.Entity<RsCostCentreTable>(entity =>
        {
            entity.ToTable("costcentre", schema: "fps");
            entity.HasKey(e => e.CostCentre);
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        modelBuilder.Entity<RsWorkGroupTable>(entity =>
        {
            entity.ToTable("workgroup", schema: "fps");
            entity.HasKey(e => e.WorkGroup);
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
        });

        modelBuilder.Entity<RsMonthlyOutputTable>(entity =>
        {
            entity.ToTable("monthlyoutput", schema: "fps");
            entity.HasKey(e => new { e.Buyer, e.WorkGroup, e.TestCode, e.Month });
            entity.Property(e => e.Buyer).HasColumnName("buyer");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.TestCode).HasColumnName("testcode");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Volume).HasColumnName("volume");
        });

        modelBuilder.Entity<RsTlkpTestReqmtTable>(entity =>
        {
            entity.ToTable("tlkptestreqmt", schema: "fps");
            entity.HasKey(e => new { e.ProjectBuyerCode, e.TestCode });
            entity.Property(e => e.ProjectBuyerCode).HasColumnName("projectbuyercode");
            entity.Property(e => e.TestCode).HasColumnName("testcode");
            entity.Property(e => e.UnitPrice).HasColumnName("unitprice");
        });

        modelBuilder.Entity<RsPeriodMonthlyOutputTable>(entity =>
        {
            entity.ToTable("period_monthlyoutput", schema: "fps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Period).HasColumnName("period");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.Opc).HasColumnName("opc");
            entity.Property(e => e.Occ).HasColumnName("occ");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Spc).HasColumnName("spc");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.Scc).HasColumnName("scc");
            entity.Property(e => e.TestCode).HasColumnName("testcode");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.TestPrice).HasColumnName("testprice");
            entity.Property(e => e.TotalCost).HasColumnName("totalcost");
        });

        modelBuilder.Entity<RsProjSubContractTable>(entity =>
        {
            entity.ToTable("proj_subcontract", schema: "fps");
            entity.HasKey(e => new { e.SubContCounter, e.FpsYear });
            entity.Property(e => e.SubContCounter).HasColumnName("subcontcounter");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.Month).HasColumnName("month").HasConversion<double?>();
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.AcctCode).HasColumnName("acctcode");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        modelBuilder.Entity<RsPeriodProjSubContractTable>(entity =>
        {
            entity.ToTable("period_proj_subcontract", schema: "fps");
            entity.HasKey(e => new { e.Period, e.SubContCounter });
            entity.Property(e => e.Period).HasColumnName("period");
            entity.Property(e => e.SubContCounter).HasColumnName("subcontcounter");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.Opc).HasColumnName("opc");
            entity.Property(e => e.Occ).HasColumnName("occ");
            entity.Property(e => e.Month).HasColumnName("month").HasConversion<double>();
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.AcctCode).HasColumnName("acctcode");
        });

        modelBuilder.Entity<RsTblWgEmployeeTable>(entity =>
        {
            entity.ToTable("tblwgemployee", schema: "fps");
            entity.HasKey(e => e.PactId);
            entity.Property(e => e.PactId).HasColumnName("pactid");
            entity.Property(e => e.SpNumber).HasColumnName("spnumber");
        });

        modelBuilder.Entity<RsPeriodTimeCostCalcsTable>(entity =>
        {
            entity.ToTable("period_timecostcalcs", schema: "fps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Period).HasColumnName("period");
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode");
            entity.Property(e => e.Month).HasColumnName("month").HasConversion<double>();
            entity.Property(e => e.DefraProject).HasColumnName("defraproject");
            entity.Property(e => e.Occ).HasColumnName("occ");
            entity.Property(e => e.Opc).HasColumnName("opc");
            entity.Property(e => e.Spc).HasColumnName("spc");
            entity.Property(e => e.Scc).HasColumnName("scc");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.GradeCode).HasColumnName("gradecode");
            entity.Property(e => e.SpNumber).HasColumnName("spnumber");
            entity.Property(e => e.ChargeRate).HasColumnName("chargerate");
            entity.Property(e => e.Pay).HasColumnName("pay").HasColumnType("money");
            entity.Property(e => e.NonPay).HasColumnName("nonpay").HasColumnType("money");
            entity.Property(e => e.Overhead).HasColumnName("overhead").HasColumnType("money");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.TotalCost).HasColumnName("totalcost").HasColumnType("money");
        });

        modelBuilder.Entity<RsRecreateSummariesLogTable>(entity =>
        {
            entity.ToTable("recreatesummaries_log", schema: "fps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.Period).HasColumnName("period").HasConversion<short>();
            entity.Property(e => e.DateDone).HasColumnName("datedone");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        });

        // Views consumed by RecreateSummaries
        modelBuilder.Entity<RsQryTotalAdditionalCostsView>(entity =>
        {
            entity.ToView("qrytotaladditionalcosts", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.TotalAdditionalCosts).HasColumnName("totaladditionalcosts");
        });

        modelBuilder.Entity<RsQryTotalAnimalCostsView>(entity =>
        {
            entity.ToView("qrytotalanimalcosts", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.TotalAnimalCosts).HasColumnName("totalanimalcosts");
        });

        modelBuilder.Entity<RsQryTotalStaffCostsView>(entity =>
        {
            entity.ToView("qrytotalstaffcosts", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.TotalStaffCosts).HasColumnName("totalstaffcosts");
            entity.Property(e => e.TotalPayCosts).HasColumnName("totalpaycosts");
        });

        modelBuilder.Entity<RsQryTotalTestCostsView>(entity =>
        {
            entity.ToView("qrytotaltestcosts", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.TotalTestCosts).HasColumnName("totaltestcosts");
        });

        modelBuilder.Entity<RsQryProjectMonthCwView>(entity =>
        {
            entity.ToView("qryprojectmonthcw", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.MonthNo)
                  .HasColumnName("monthno")
                  .HasConversion<double>();
            entity.Property<int?>("FpsYear").HasColumnName("fpsyear");
            entity.Property(e => e.CwDebit).HasColumnName("cwdebit");
            entity.Property(e => e.CwCredit).HasColumnName("cwcredit");
        });

        modelBuilder.Entity<RsVpactTblStaffView>(entity =>
        {
            entity.ToView("vpacttblstaff", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.PactId).HasColumnName("pactid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.WorkGroupGrade).HasColumnName("workgroupgrade");
        });

        modelBuilder.Entity<RsQryJobMonthSubContractsView>(entity =>
        {
            entity.ToView("qryjobmonth_subcontracts", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.Month)
                  .HasColumnName("month")
                  .HasConversion<double>();
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.Animals).HasColumnName("animals");
            entity.Property(e => e.Other).HasColumnName("other");
        });

        modelBuilder.Entity<RsQryJobMonthTimeView>(entity =>
        {
            entity.ToView("qryjobmonth_time", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.Month)
                  .HasColumnName("month")
                  .HasConversion<double>();
            entity.Property(e => e.SumOfCost).HasColumnName("sumofcost");
            entity.Property(e => e.SumOfHours).HasColumnName("sumofhours");
            entity.Property(e => e.SumOfPayRate).HasColumnName("sumofpayrate");
        });

        modelBuilder.Entity<RsQryJobMonthMilestoneView>(entity =>
        {
            entity.ToView("qryjobmonthmilestone", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.DueMonth)
                  .HasColumnName("duemonth")
                  .HasConversion<double>();
            entity.Property(e => e.MstoneDue).HasColumnName("mstonedue");
            entity.Property(e => e.DueDone).HasColumnName("due__done");
            entity.Property(e => e.OnTime).HasColumnName("ontime");
        });

        modelBuilder.Entity<RsQryJobMonthTransfersTotalView>(entity =>
        {
            entity.ToView("qryjobmonth_transferstotal", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.Month)
                  .HasColumnName("month")
                  .HasConversion<double>();
            entity.Property(e => e.SumOfTransferCost).HasColumnName("sumoftransfercost");
        });

        modelBuilder.Entity<RsQryJobMonthInvoicesView>(entity =>
        {
            entity.ToView("qryjobmonth_invoices", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.ProjectParent).HasColumnName("projectparent");
            entity.Property(e => e.Month)
                  .HasColumnName("month")
                  .HasConversion<double>();
            entity.Property(e => e.SumOfAmount1).HasColumnName("sumofamount1");
            entity.Property(e => e.WorkCost).HasColumnName("workcost");
        });

        modelBuilder.Entity<RsQryJobMonthPortfolioSalesView>(entity =>
        {
            entity.ToView("qryjobmonthportfoliosales", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.PlanPortfolio).HasColumnName("planportfolio");
            entity.Property(e => e.Month)
                  .HasColumnName("month")
                  .HasConversion<double>();
            entity.Property(e => e.Fee).HasColumnName("fee");
        });

        modelBuilder.Entity<RsQryJobMonthTotProfileView>(entity =>
        {
            entity.ToView("qryjobmonth_totprofile", schema: "fps");
            entity.HasNoKey();
            entity.Property(e => e.Project).HasColumnName("project");
            entity.Property(e => e.SumOfCostProfile).HasColumnName("sumofcostprofile").HasColumnType("money");
        });
    }
}
