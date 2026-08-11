using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Data
{
    public partial class FpsDbContext : DbContext
    {
        private readonly IFpsRequestContext _fpsRequestContext;
        public int FilterFpsYear => _fpsRequestContext.FpsYear;

        public FpsDbContext(DbContextOptions<FpsDbContext> options, IFpsRequestContext fpsRequestContext)
            : base(options)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<JobCode> JobCodes { get; set; }
        public virtual DbSet<TimeCodeValid> TimeCodeValids { get; set; }
        public virtual DbSet<WorkGroup> WorkGroups { get; set; }
        public virtual DbSet<ProjectInvoice> ProjectInvoices { get; set; }
        public virtual DbSet<SummarisedWgTimeView> SummarisedWgTimeViews { get; set; }
        public virtual DbSet<ProjectSubContract> ProjectSubContracts { get; set; }
        public virtual DbSet<TestCapability> TestCapabilities { get; set; }
        public virtual DbSet<TestRequirement> TestRequirements { get; set; }
       
        public virtual DbSet<TestorProduct> TestorProducts { get; set; }
        public virtual DbSet<Month> Months { get; set; }
        public virtual DbSet<TestRequirementLog> TestRequirementLogs { get; set; }
        public virtual DbSet<MonthlyOutput> MonthlyOutputs { get; set; }
        public virtual DbSet<MonthlyOutputLog> MonthlyOutputLogs { get; set; }
        public virtual DbSet<StagingMonthlyOutput> StagingMonthlyOutputs { get; set; }
        public virtual DbSet<MonthlyTimeLog> MonthlyTimeLogs { get; set; }
        public virtual DbSet<MonthlyTime> MonthlyTimes { get; set; }
        public virtual DbSet<StagingMonthlyTime> StagingMonthlyTimes { get; set; }
        public virtual DbSet<MonthlyInvoicesSummary> MonthlyInvoicesSummary { get; set; }
        public virtual DbSet<MonthlySubContractsSummary> MonthlySubContractsSummary { get; set; }
        public virtual DbSet<ProjectMonth> ProjectMonths { get; set; }
        public virtual DbSet<ProjectMonthFinal> ProjectMonthFinals { get; set; }
        public virtual DbSet<PeriodMonth> PeriodMonths { get; set; }
        public virtual DbSet<CalenderMonth> CalenderMonths { get; set; }
        public virtual DbSet<PactWorkGroupGradeView> PactWorkGroupGradeViews { get; set; }
        public virtual DbSet<WorkGroupStaffView> WorkGroupStaffViews { get; set; }
        public virtual DbSet<WgSummarisedStaffTimeUsageView> WgSummarisedStaffTimeUsageViews { get; set; }
        public virtual DbSet<WorkGroupStaffView> PactStaffViews { get; set; }
        public virtual DbSet<PactProfitCentreView> PactProfitCentreViews { get; set; }
        public virtual DbSet<ProfitCentre> ProfitCentres { get; set; }
        public virtual DbSet<WorkGroupView> WorkGroupViews { get; set; }
        public virtual DbSet<RecreateSummaryLog> RecreateSummaryLogs { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<ReleasePeriod> ReleasePeriods { get; set; }
        public virtual DbSet<ProjectView> ProjectViews { get; set; }
        public virtual DbSet<TimeCostCalcs> TimeCostCalcs { get; set; }
        public virtual DbSet<Program> Programs { get; set; }
        public virtual DbSet<TestReqBreakdownView> TestReqBreakdownViews { get; set; }
        public virtual DbSet<ProjectSubcontractStaging> ProjectSubcontractStagings { get; set; }
        public virtual DbSet<TestActualBreakdownView> TestActualBreakdownViews { get; set; }
        public virtual DbSet<TestPlanCostBreakdownView> TestPlanCrossTabViews { get; set; }

        public virtual DbSet<BatchJobMaster> BatchJobs { get; set; }
        public virtual DbSet<BatchJobQueue> BatchJobQueues { get; set; }
        public virtual DbSet<BatchJobQueueLog> BatchJobQueueLogs { get; set; }
        public virtual DbSet<BatchJobStatus> BatchJobStatuses { get; set; }
        public virtual DbSet<WorkGroupGeneralView> WorkGroupGeneralViews { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProjectMap());
            modelBuilder.Entity<Project>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new WorkGroupMap());
            modelBuilder.Entity<WorkGroup>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TimeCodeValidMap());
            modelBuilder.Entity<TimeCodeValid>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new JobCodeMap());
            modelBuilder.Entity<JobCode>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestCapabilityMap());
            modelBuilder.Entity<TestCapability>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestRequirementMap());
            modelBuilder.Entity<TestRequirement>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestorProductMap());
            modelBuilder.Entity<TestorProduct>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestRequirementLogMap());
            modelBuilder.Entity<TestRequirementLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthlyOutputMap());
            modelBuilder.Entity<MonthlyOutput>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthlyOutputLogMap());
            modelBuilder.Entity<MonthlyOutputLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectInvoiceMap());
            modelBuilder.Entity<ProjectInvoice>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectSubContractMap());
            modelBuilder.Entity<ProjectSubContract>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthlyTimeLogMap());
            modelBuilder.Entity<MonthlyTimeLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthlyTimeMap());
            modelBuilder.Entity<MonthlyTime>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new StagingMonthlyTimeMap());
            modelBuilder.ApplyConfiguration(new StagingMonthlyOutputMap());

            modelBuilder.ApplyConfiguration(new MonthMap());

            modelBuilder.ApplyConfiguration(new MonthlyInvoicesSummaryMap());
            modelBuilder.Entity<MonthlyInvoicesSummary>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthlySubContractSummaryMap());
            modelBuilder.Entity<MonthlySubContractsSummary>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectMonthFinalMap());
            modelBuilder.Entity<ProjectMonthFinal>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectMonthMap());
            modelBuilder.Entity<ProjectMonth>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new PeriodMonthMap());
            modelBuilder.Entity<PeriodMonth>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new CalenderMonthMap());

            modelBuilder.ApplyConfiguration(new PactWorkGroupGradeViewMap());
            modelBuilder.Entity<PactWorkGroupGradeView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new WorkGroupStaffViewMap());
            modelBuilder.Entity<WorkGroupStaffView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new WgSummarisedStaffTimeUsageViewMap());
            modelBuilder.Entity<WgSummarisedStaffTimeUsageView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new SummarisedWgTimeViewMap());
            modelBuilder.Entity<SummarisedWgTimeView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new PactProfitCentreViewMap());

            modelBuilder.ApplyConfiguration(new WorkGroupViewMap());
            modelBuilder.Entity<WorkGroupView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProfitCentreMap());

            modelBuilder.ApplyConfiguration(new UserMap());

            modelBuilder.ApplyConfiguration(new RecreateSummaryLogMap());
            modelBuilder.Entity<RecreateSummaryLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new SettingsMap());

            modelBuilder.ApplyConfiguration(new ReleasePeriodMap());
            modelBuilder.Entity<ReleasePeriod>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectViewMap());
            modelBuilder.Entity<ProjectView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TimeCostCalcsMap());
            modelBuilder.Entity<TimeCostCalcs>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProgramMap());
            modelBuilder.Entity<Program>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);
            modelBuilder.ApplyConfiguration(new TestReqBreakdownViewMap());
            modelBuilder.Entity<TestReqBreakdownView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);
            modelBuilder.ApplyConfiguration(new TestActualBreakdownViewMap());
            modelBuilder.Entity<TestActualBreakdownView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);
            modelBuilder.ApplyConfiguration(new TestPlanCostBreakdownMap());
            modelBuilder.Entity<TestPlanCostBreakdownView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectSubcontractStagingMap());
            modelBuilder.ApplyConfiguration(new BatchJobMasterMap());
            modelBuilder.ApplyConfiguration(new BatchJobQueueMap());
            modelBuilder.Entity<BatchJobQueue>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);
            modelBuilder.ApplyConfiguration(new BatchJobQueueLogMap());
            modelBuilder.ApplyConfiguration(new BatchJobStatusMap());
            modelBuilder.ApplyConfiguration(new WorkGroupGeneralViewMap());
            modelBuilder.Entity<WorkGroupGeneralView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);
        }
    }
}