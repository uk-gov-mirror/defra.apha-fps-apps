using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

/*
 * TRANSFORMENGINE MIGRATION — FpsDbContext.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - Added DbSet<ContributionSummary> ContributionSummaries property for frmTimeSellerPC migration.
 *   - Applied ContributionSummaryMap configuration in OnModelCreating.
 *   - Added HasQueryFilter on ContributionSummary scoped to FilterFpsYear (same pattern as all
 *     other year-partitioned entities in this context).
 *   - Registered ContributionSummaryTotals as a keyless entity (HasNoKey) per the entity
 *     documentation requirement; no DbSet needed — used only for LINQ projection results.
 *
 * PRESERVED:
 *   - All existing DbSet properties and OnModelCreating registrations untouched.
 *   - IFpsRequestContext injection and FilterFpsYear pattern unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify that fps.tblkpcontributionsummary table exists in the
 *     PostgreSQL schema before applying EF Core migrations. ContributionSummaryMap maps to
 *     this table; confirm DDL with DBA.
 */

namespace Apha.FPS.DataAccess.Data
{
    public partial class FpsDbContext : DbContext
    {
        private readonly IFpsRequestContext _fPSYearContext;
        public int FilterFpsYear => _fPSYearContext.FpsYear;

        public FpsDbContext(DbContextOptions<FpsDbContext> options, IFpsRequestContext fPSYearContext)
            : base(options)
        {
            _fPSYearContext = fPSYearContext;
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserProgram> UserPrograms { get; set; }
        public virtual DbSet<Program> Programs { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<StaffJob> StaffJobs { get; set; }
        public virtual DbSet<WorkGroupEmployee> WorkGroupEmployees { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<FpsSetting> TblSettings { get; set; }
        public virtual DbSet<Workgroup> Workgroups { get; set; }
        public virtual DbSet<WorkgroupGrade> WorkgroupGrades { get; set; }
        public virtual DbSet<ProfitCentreGrade> ProfitCentreGrades { get; set; }
        public virtual DbSet<DivisionGrade> DivisionGrades { get; set; }
        public virtual DbSet<Grade> Grades { get; set; }
        public virtual DbSet<UserProfitcentre> UserProfitcentres { get; set; }
        public virtual DbSet<ProfitCentre> ProfitCentres { get; set; }
        public virtual DbSet<JobCode> JobCodes { get; set; }
        public virtual DbSet<Status> Statuses { get; set; }
        public virtual DbSet<Disease> Diseases { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<Animal> Animals { get; set; }
        public virtual DbSet<AnimalRequest> AnimalRequests { get; set; }
        public virtual DbSet<ProjectGroup> ProjectGroups { get; set; }
        public virtual DbSet<ProjectGroupView> ProjectGroupViews { get; set; }
        public virtual DbSet<ContractView> ContractViews { get; set; }
        public virtual DbSet<AccountCode> AccountCodes { get; set; }
        public virtual DbSet<SubAccount> SubAccounts { get; set; }
        public virtual DbSet<UserCategory> UserCategories { get; set; }
        public virtual DbSet<StaffActiveView> StaffActiveView { get; set; }
        public virtual DbSet<WorkgroupGradeGeneralView> WorkgroupGradeGeneralViews { get; set; }

        public virtual DbSet<ProgramView> ProgramViews { get; set; }
        public virtual DbSet<ProjectView> ProjectViews { get; set; }
        public virtual DbSet<StaffJobTblView> StaffJobTblViews { get; set; }
        public virtual DbSet<StaffGeneralView> StaffGeneralViews { get; set; }
        public virtual DbSet<StaffView> StaffViews { get; set; }
        public virtual DbSet<StaffPickView> StaffPickViews { get; set; }
        public virtual DbSet<AnimalRequestView> AnimalRequestViews { get; set; }
        public virtual DbSet<PactProjectView> PactProjectViews { get; set; }
        public virtual DbSet<PactWorkGroupGradeView> PactWorkGroupGradeViews { get; set; }
        public virtual DbSet<YearMaster> YearMasters { get; set; }
        public virtual DbSet<StaffJobLog> StaffJobLogs { get; set; }
        public virtual DbSet<AnimalRequestLog> AnimalRequestLogs { get; set; }
        public virtual DbSet<MonthlyTimeLog> MonthlyTimeLogs { get; set; }
        public virtual DbSet<MonthlyOutputLog> MonthlyOutputLogs { get; set; }
        public virtual DbSet<AdditionalCostLog> AdditionalCostLogs { get; set; }
        public virtual DbSet<TestRequirementLog> TestRequirementLogs { get; set; }
        public virtual DbSet<SurvFFSubmission> SurvFFSubmissions { get; set; }
        public virtual DbSet<ProjectLog> ProjectLogs { get; set; }
        public virtual DbSet<TestRequirement> TestRequirements { get; set; }
        public virtual DbSet<AdditionalCost> AdditionalCosts { get; set; }
        public virtual DbSet<TestCapability> TestCapabilities { get; set; }
        public virtual DbSet<MonthlyTime> MonthlyTimes { get; set; }
        public virtual DbSet<ProjectMonth> ProjectMonths { get; set; }
        public virtual DbSet<TimeCodeValid> TimeCodeValids { get; set; }
        public virtual DbSet<Milestone> Milestones { get; set; }
        public virtual DbSet<ProjectMonthFinal> ProjectMonthFinals { get; set; }
        public virtual DbSet<ProjectInvoice> ProjectInvoices { get; set; }
        public virtual DbSet<ProjectSubContract> ProjectSubContracts { get; set; }

        public virtual DbSet<TimeCostCalcsView> TimeCostCalcsViews { get; set; }
        public virtual DbSet<TimeCostCalcs> TimeCostCalcs { get; set; }

        public virtual DbSet<MonthlyOutput> MonthlyOutputs { get; set; }

        public virtual DbSet<Division> Divisions { get; set; }
        public virtual DbSet<Agency> Agencies { get; set; }

        public virtual DbSet<AdditionalCostView> AdditionalCostViews { get; set; }
        public virtual DbSet<AccountCategory> AccountCategories { get; set; }
        public virtual DbSet<WorkGroupStaff> WorkGroupStaffs { get; set; }
        public virtual DbSet<ProfitCentreView> ProfitCentreViews { get; set; }
        public virtual DbSet<ProfitCentreGradeView> ProfitCentreGradeViews { get; set; }
        public virtual DbSet<WorkGroupGradeView> WorkGroupGradeViews { get; set; }
        public virtual DbSet<WorkGroupEmployeeView> WorkGroupEmployeeViews { get; set; }
        public virtual DbSet<PactStaff> PactStaffs { get; set; }


        public virtual DbSet<ProjectStaffPlanView> ProjectStaffPlanViews { get; set; }
        public virtual DbSet<ProjectGroupStaffPlanView> ProjectGroupStaffPlanViews { get; set; }

        // TRANSFORMENGINE: ContributionSummaries — frmTimeSellerPC grid data; backed by fps.tblkpcontributionsummary
        public virtual DbSet<ContributionSummary> ContributionSummaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserMap());

            modelBuilder.ApplyConfiguration(new UserProgramMap());
            modelBuilder.Entity<UserProgram>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProgramMap());
            modelBuilder.Entity<Program>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectMap());
            modelBuilder.Entity<Project>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new StaffJobMap());
            modelBuilder.Entity<StaffJob>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new WgEmployeeMap());
            modelBuilder.Entity<WorkGroupEmployee>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new EmployeeMap());
            modelBuilder.Entity<Employee>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new FpsSettingMap());
            modelBuilder.Entity<FpsSetting>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new WorkgroupMap());
            modelBuilder.Entity<Workgroup>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new WorkgroupGradeMap());
            modelBuilder.Entity<WorkgroupGrade>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProfitCentreGradeMap());
            modelBuilder.Entity<ProfitCentreGrade>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new UserProfitcentreMap());
            modelBuilder.Entity<UserProfitcentre>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProfitCentreMap());
            modelBuilder.ApplyConfiguration(new ProfitCentreViewMap());
            modelBuilder.ApplyConfiguration(new ProfitCentreGradeViewMap());
            modelBuilder.Entity<ProfitCentreGradeView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);
            modelBuilder.ApplyConfiguration(new WorkGroupGradeViewMap());
            modelBuilder.Entity<WorkGroupGradeView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);  
            modelBuilder.ApplyConfiguration(new WorkGroupEmployeeViewMap());
            modelBuilder.Entity<WorkGroupEmployeeView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new JobCodeMap());
            modelBuilder.Entity<JobCode>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new StatusMap());
            modelBuilder.ApplyConfiguration(new DiseaseMap());
            modelBuilder.ApplyConfiguration(new CustomerMap());

            modelBuilder.ApplyConfiguration(new ContractMap());
            modelBuilder.Entity<Contract>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new AnimalMap());
            modelBuilder.Entity<Animal>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new AnimalRequestMap());
            modelBuilder.Entity<AnimalRequest>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectGroupMap());

            modelBuilder.ApplyConfiguration(new ProjectGroupViewMap());

            modelBuilder.ApplyConfiguration(new ContractViewMap());
            modelBuilder.Entity<ContractView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new AccountCodeMap());
            modelBuilder.ApplyConfiguration(new SubAccountMap());

            modelBuilder.ApplyConfiguration(new UserCategoryMap());
            modelBuilder.Entity<UserCategory>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new StaffActiveViewMap());
            modelBuilder.Entity<StaffActiveView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new WorkgroupGradeGeneralViewMap());
            modelBuilder.Entity<WorkgroupGradeGeneralView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProgramViewMap());
            modelBuilder.Entity<ProgramView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectViewMap());
            modelBuilder.Entity<ProjectView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new StaffJobTblViewMap());
            modelBuilder.Entity<StaffJobTblView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new StaffGeneralViewMap());
            modelBuilder.Entity<StaffGeneralView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new StaffViewMap());
            modelBuilder.Entity<StaffView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new StaffPickViewMap());
            modelBuilder.Entity<StaffPickView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new AnimalRequestViewMap());
            modelBuilder.Entity<AnimalRequestView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new PactProjectViewMap());
            modelBuilder.Entity<PactProjectView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new PactWorkGroupGradeViewMap());
            modelBuilder.Entity<PactWorkGroupGradeView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new YearMasterMap());
            modelBuilder.ApplyConfiguration(new StaffJobLogMap());
            modelBuilder.Entity<StaffJobLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new AnimalRequestLogMap());
            modelBuilder.Entity<AnimalRequestLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthlyTimeLogMap());

            modelBuilder.ApplyConfiguration(new MonthlyOutputLogMap());
            modelBuilder.Entity<MonthlyOutputLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new AdditionalCostLogMap());
            modelBuilder.Entity<AdditionalCostLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);


            modelBuilder.ApplyConfiguration(new SurvFFSubmissionMap());
            modelBuilder.Entity<SurvFFSubmission>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectLogMap());
            modelBuilder.Entity<ProjectLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestRequirementMap());
            modelBuilder.Entity<TestRequirement>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestRequirementLogMap());
            modelBuilder.Entity<TestRequirementLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectInvoiceMap());
            modelBuilder.Entity<ProjectInvoice>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectSubContractMap());
            modelBuilder.Entity<ProjectSubContract>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new AdditionalCostMap());
            modelBuilder.Entity<AdditionalCost>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestCapabilityMap());
            modelBuilder.Entity<TestCapability>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthlyOutputMap());
            modelBuilder.Entity<MonthlyOutput>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            // ── Cross-year tables: no HasQueryFilter — bulk rename operates across all years ──

            modelBuilder.ApplyConfiguration(new MonthlyTimeMap());

            modelBuilder.ApplyConfiguration(new ProjectMonthMap());

            modelBuilder.ApplyConfiguration(new TimeCodeValidMap());

            modelBuilder.ApplyConfiguration(new MilestoneMap());

            modelBuilder.ApplyConfiguration(new ProjectMonthFinalMap());


            modelBuilder.ApplyConfiguration(new TimeCostCalcsViewMap());
            modelBuilder.Entity<TimeCostCalcsView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectStaffPlanViewMap());
            modelBuilder.Entity<ProjectStaffPlanView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectGroupStaffPlanViewMap());
            modelBuilder.Entity<ProjectGroupStaffPlanView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TimeCostCalcsMap());
            modelBuilder.Entity<TimeCostCalcs>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new DivisionMap());
            modelBuilder.ApplyConfiguration(new DivisionGradeMap());
            modelBuilder.Entity<DivisionGrade>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);
            modelBuilder.ApplyConfiguration(new GradeCodeMap());

            modelBuilder.ApplyConfiguration(new AdditionalCostViewMap());
            modelBuilder.Entity<AdditionalCostView>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new AccountCategoryMap());
            modelBuilder.Entity<AccountCategory>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new WorkGroupStaffMap());
            modelBuilder.Entity<WorkGroupStaff>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthlyTimeMap());
            modelBuilder.Entity<MonthlyTime>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new PactStaffMap());
            modelBuilder.Entity<PactStaff>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);
            modelBuilder.ApplyConfiguration(new MonthlyOutputMap());
            modelBuilder.Entity<MonthlyOutput>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new GradeMap());
            modelBuilder.Entity<Grade>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            // TRANSFORMENGINE: ContributionSummary map + FpsYear query filter — frmTimeSellerPC Phase 4
            modelBuilder.ApplyConfiguration(new ContributionSummaryMap());
            modelBuilder.Entity<ContributionSummary>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            // TRANSFORMENGINE: ContributionSummaryTotals — keyless aggregate projection; no DbSet required
            modelBuilder.Entity<ContributionSummaryTotals>().HasNoKey();
        }
    }
}