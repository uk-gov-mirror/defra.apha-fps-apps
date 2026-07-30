using Apha.Common.Utilities.ExcelExport;
using Apha.Common.Utilities.ExcelImport;
using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Services.Costbook;
using Apha.FPSApps.Application.Services.FPS;
using Apha.FPSApps.Application.Services.PACT;
using Apha.FPSApps.Application.Services.PIMS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Web.Handler;

namespace Apha.FPSApps.Web.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddServices();
            services.AddRepositories();
            return services;
        }
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IStaffJobService, StaffJobService>();
            services.AddTransient<RequestHeadersHandler>();
            services.AddScoped<IFpsYearContext, FpsYearContext>();
            services.AddScoped<IProgramService, ProgramService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectJobCodeService, ProjectJobCodeService>();
            services.AddScoped<IPactTimeCodeValidService, PactTimeCodeValidService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IAnimalPlanService, AnimalPlanService>();
            services.AddScoped<ISettingService, SettingService>();
            // CostBook services - Following FPS pattern
            services.AddScoped<ICostBookProjectService, CostBookProjectService>();
            services.AddScoped<ICostBookCustomerService, CostBookCustomerService>();
            services.AddScoped<ICostBookDiseaseService, CostBookDiseaseService>();
            services.AddScoped<ICostBookProgramService, CostBookProgramService>();
            services.AddScoped<ICostBookStaffService, CostBookStaffService>();
            services.AddScoped<ICostBookContractService, CostBookContractService>();
            services.AddScoped<ICostBookYearlyDetailsService, CostBookYearlyDetailsService>();
            services.AddScoped<ICostBookProjectSummaryService, CostBookProjectSummaryService>();
            services.AddScoped<ICostBookSettingsService, CostBookSettingsService>();
            // frmMaintCostCentres frontend CRUD service; AddScoped follows request-scoped API client pattern
            services.AddScoped<ICostCentreService, CostCentreService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IYearMasterService, YearMasterService>();
            services.AddScoped<IDivisionService, DivisionService>();
            services.AddScoped<IWorkGroupGradeService, WorkGroupGradeService>();
            services.AddScoped<IProjectInvoiceService, ProjectInvoiceService>();
            services.AddScoped<IProjectSubContractService, ProjectSubContractService>();
            services.AddScoped<IMonthService, MonthService>();
            services.AddScoped<ICalenderMonthService, CalenderMonthService>();
            services.AddScoped<ITestCapabilityService, TestCapabilityService>();
            services.AddScoped<ITestRequirementService, TestRequirementService>();
            //   TestListVlaService delegates to IPactApiClient.PactTestList (PACT API).
            //   FpsYear filtering is handled by the PACT DbContext global query filter.
            services.AddScoped<ITestListVlaService, TestListVlaService>();
            services.AddScoped<ITimeCostCalcsService, TimeCostCalcsService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IExcelImportService, ExcelImportService>();
            services.AddScoped<IAppStateService, AppStateService>();
            services.AddScoped<IAdditionalCostService, AdditionalCostService>();
            services.AddScoped<IAccountCategoryService, AccountCategoryService>();
            services.AddScoped<ICostBookAccountGroupService, CostBookAccountGroupService>();
            services.AddScoped<ICostBookCapsStaffService, CostBookCapsStaffService>();
            services.AddScoped<ICostBookMaintenanceService, CostBookMaintenanceService>();
            // PIMS
            services.AddScoped<IProjectListService, ProjectListService>();
            services.AddScoped<IProjectDetailsService, ProjectDetailsService>();
            services.AddScoped<IProjectCommentService, ProjectCommentService>();
            services.AddScoped<IProposedProjectService, ProposedProjectService>();
            services.AddScoped<IProjectYearCostsService, ProjectYearCostsService>();
            services.AddScoped<IMilestoneService, MilestoneService>();
            services.AddScoped<IRadTrackInvoiceService, RadTrackInvoiceService>();
         
            services.AddScoped<IYearlyFinancialDataService, YearlyFinancialDataService>();

            services.AddScoped<IProfitCentreService, ProfitCentreService>();
            services.AddScoped<IProfitCentreGradeService, ProfitCentreGradeService>();
            services.AddScoped<IWorkGroupGradeService, WorkGroupGradeService>();
            services.AddScoped<IWorkGroupEmployeeService, WorkGroupEmployeeService>();
            services.AddScoped<ITestorProductService, TestorProductService>();
            services.AddScoped<IMonthlyOutputService, MonthlyOutputService>();
            services.AddScoped<IPactMonthlyOutputService, PactMonthlyOutputService>();
            services.AddScoped<IPactMonthlyTimeService, PactMonthlyTimeService>();
            services.AddScoped<IProjectProfileService, ProjectProfileService>();
            services.AddScoped<IProjectMonthService, ProjectMonthService>();
            services.AddScoped<ICalenderMonthService, CalenderMonthService>();
            services.AddScoped<IWorkGroupReportEmailService, WorkGroupReportEmailService>();
            // TRANSFORMENGINE: IWorkgroupMaintenanceService registered — Phase 10 (Step 15c)
            // FPS CRUD maintenance service for frmMaintWorkGroup2 (distinct from PACT IWorkGroupService read-only lookup)
            services.AddScoped<IWorkgroupMaintenanceService, WorkgroupMaintenanceService>();
            services.AddScoped<Apha.FPSApps.Application.Interfaces.PACT.IWorkGroupService, Apha.FPSApps.Application.Services.PACT.WorkGroupService>();
            services.AddScoped<IDivisionGradeService, DivisionGradeService>();
            services.AddScoped<IProjectStaffPlanService, ProjectStaffPlanService>();
            services.AddScoped<IProjectStaffPlanDetailsService, ProjectStaffPlanDetailsService>();
            services.AddScoped<ITestReqBreakdownService, TestReqBreakdownService>();
            services.AddScoped<ITestActualBreakdownService, TestActualBreakdownService>();
            services.AddScoped<ITestPlanCrossTabService, TestPlanCrossTabService>();
            services.AddScoped<IProjectGroupStaffPlanService, ProjectGroupStaffPlanService>();
            services.AddScoped<ISummarisedWorkgroupTimeService, SummarisedWgTimeService>();
            services.AddScoped<IAnimalService, AnimalService>();
            services.AddScoped<Apha.FPSApps.Application.Interfaces.FPS.IUserService, Apha.FPSApps.Application.Services.FPS.UserService>();
            services.AddScoped<IRecreateSummaryService, RecreateSummaryService>();
            services.AddScoped<IBudgetBidsService, BudgetBidsService>();
            services.AddScoped<IPurchasesService, PurchasesService>();
            services.AddScoped<ITotalBusinessOverheadsService, TotalBusinessOverheadsService>();
            services.AddScoped<IReleaseSummaryService, ReleaseSummaryService>();
            services.AddScoped<IPlanStaffZTCodeService, PlanStaffZTCodeService>();
            services.AddScoped<IContributionSummaryService, ContributionSummaryService>();
            services.AddScoped<IProjectAuditTrailService, ProjectAuditTrailService>();
            services.AddScoped<IBosworthInterfaceService, BosworthInterfaceService>();
            services.AddScoped<IResourceAllocationService, ResourceAllocationService>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            //   used by IFpsProfitCentreApiClient and other IFps*ApiClient registrations (see ApiClientExtension.cs).
            services.AddScoped<IFpsProjectAuditTrailApiClient, FpsProjectAuditTrailApiClient>();
            return services;
        }
    }
}
