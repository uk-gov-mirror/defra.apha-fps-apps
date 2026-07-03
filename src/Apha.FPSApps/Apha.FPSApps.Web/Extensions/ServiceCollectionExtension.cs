/*
 * TRANSFORMENGINE MIGRATION — ServiceCollectionExtension.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - Added services.AddScoped<IAsuViewService, AsuViewService>() in AddServices()
 *     for the ASU View resource family (frontend thin-delegate service)
 *   - Placed alphabetically among the FPS service registrations (after IAnimalPlanService,
 *     before IBudgetBidsService)
 *   - IFpsAsuViewApiClient is NOT registered separately here — FpsAsuViewApiClient is
 *     composed directly inside FpsApiClient (new FpsAsuViewApiClient(http, mapper)) so
 *     it is resolved via IFpsApiClient, not the DI container independently
 *
 * PRESERVED:
 *   - All existing AddScoped registrations unchanged
 *   - AddRepositories() body unchanged (returns services; no repo registrations needed)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — IAsuViewService registration is fully automated
 */

using Apha.Common.Utilities.ExcelExport;
using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Services.Costbook;
using Apha.FPSApps.Application.Services.FPS;
using Apha.FPSApps.Application.Services.PACT;
using Apha.FPSApps.Application.Services.PIMS;
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
            services.AddScoped<ITimeCostCalcsService, TimeCostCalcsService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IAppStateService, AppStateService>();
            services.AddScoped<IAdditionalCostService, AdditionalCostService>();
            services.AddScoped<IAccountCategoryService, AccountCategoryService>();
            // PIMS
            services.AddScoped<IProjectListService, ProjectListService>();
            services.AddScoped<IProjectDetailsService, ProjectDetailsService>();
            services.AddScoped<IProjectCommentService, ProjectCommentService>();
            services.AddScoped<IProposedProjectService, ProposedProjectService>();
            services.AddScoped<IProjectYearCostsService, ProjectYearCostsService>();
            services.AddScoped<IMilestoneService, MilestoneService>();
            services.AddScoped<IRadTrackInvoiceService, RadTrackInvoiceService>();

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
            services.AddScoped<Apha.FPSApps.Application.Interfaces.PACT.IWorkGroupService, Apha.FPSApps.Application.Services.PACT.WorkGroupService>();
            services.AddScoped<IDivisionGradeService, DivisionGradeService>();
            services.AddScoped<IProjectStaffPlanService, ProjectStaffPlanService>();
            services.AddScoped<IProjectGroupStaffPlanService, ProjectGroupStaffPlanService>();
            services.AddScoped<ISummarisedWorkgroupTimeService, SummarisedWgTimeService>();
            services.AddScoped<IAnimalService, AnimalService>();
            // TRANSFORMENGINE: IAsuViewService registered — Phase 10 (Step 15c)
            // AsuViewService is a thin delegate forwarding calls to IFpsApiClient.FpsAsuView.
            // IFpsAsuViewApiClient is NOT registered here; FpsAsuViewApiClient is composed
            // inside FpsApiClient and resolved via IFpsApiClient (registered in ApiClientExtension).
            services.AddScoped<IAsuViewService, AsuViewService>();
            services.AddScoped<Apha.FPSApps.Application.Interfaces.FPS.IUserService, Apha.FPSApps.Application.Services.FPS.UserService>();
            services.AddScoped<IRecreateAndReleaseSummaryService, RecreateAndReleaseSummaryService>();
            services.AddScoped<IBudgetBidsService, BudgetBidsService>();
            services.AddScoped<IPurchasesService, PurchasesService>();
            services.AddScoped<IReleaseSummaryService, ReleaseSummaryService>();
            services.AddScoped<IPlanStaffZTCodeService, PlanStaffZTCodeService>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services;
        }
    }
}