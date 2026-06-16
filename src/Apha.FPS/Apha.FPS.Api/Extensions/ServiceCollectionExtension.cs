/*
 * TRANSFORMENGINE MIGRATION — ServiceCollectionExtension.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - Registered IContributionSummaryService / ContributionSummaryService (scoped) in AddServices().
 *   - Registered IContributionSummaryRepository / ContributionSummaryRepository (scoped) in AddRepositories().
 *
 * PRESERVED:
 *   - All pre-existing service and repository registrations unchanged.
 *   - Method structure (AddApplicationServices, AddServices, AddRepositories) unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ContributionSummaryRepository is registered here; confirm the
 *     concrete implementation resolves without ambiguity once DataAccess project is compiled.
 */

using Apha.Common.Utilities.StateManagement;
using Apha.Common.Utilities.ExcelExport;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Context;
using Apha.FPS.DataAccess.Repositories;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

namespace Apha.FPS.Api.Extensions
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
            // Add your application services here
            services.AddScoped<IAppStateService, AppStateService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IStaffJobService, StaffJobService>();
            services.AddScoped<IFpsSettingService, FpsSettingService>();
            services.AddScoped<IAnimalService, AnimalService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IProgramService, ProgramService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IStatusService, StatusService>();
            services.AddScoped<IDiseaseService, DiseaseService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IYearMasterService, YearMasterService>();
            services.AddScoped<IProjectGroupService, ProjectGroupService>();
            services.AddScoped<ITimeCostCalcsService, TimeCostCalcsService>();
            services.AddScoped<IDivisionService, DivisionService>();
            services.AddScoped<IAgencyService, AgencyService>();
            services.AddScoped<IAdditionalCostService, AdditionalCostService>();
            services.AddScoped<IAccountCategoryService, AccountCategoryService>();
            services.AddScoped<IMonthlyOutputService, MonthlyOutputService>();
            services.AddScoped<IAccountCodeService, AccountCodeService>();
            services.AddScoped<ISubAccountService, SubAccountService>();
            services.AddScoped<IProfitCentreService, ProfitCentreService>();
            services.AddScoped<IProfitCentreGradeService, ProfitCentreGradeService>();
            services.AddScoped<IWorkGroupGradeService, WorkGroupGradeService>();
            services.AddScoped<IWorkgroupService, WorkgroupService>();
            services.AddScoped<IWorkGroupEmployeeService, WorkGroupEmployeeService>();
            services.AddScoped<IDivisionGradeService, DivisionGradeService>();
            services.AddScoped<IProjectStaffPlanService, ProjectStaffPlanService>();
            // TRANSFORMENGINE: IGradeService/GradeService registered � Phase 5 frmMaintGrade migration
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IProjectGroupStaffPlanService, ProjectGroupStaffPlanService>();
            // TRANSFORMENGINE: IContributionSummaryService/ContributionSummaryService registered — Phase 5 frmTimeSellerPC migration
            services.AddScoped<IContributionSummaryService, ContributionSummaryService>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Add your data access services here
            services.AddScoped<IFpsRequestContext, FpsRequestContext>();
            services.AddScoped<IProgramRepository, ProgramRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IStaffJobRepository, StaffJobRepository>();
            services.AddScoped<IFpsSettingRepository, FpsSettingRepository>(); 
            services.AddScoped<IAnimalRepository, AnimalRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IStatusRepository, StatusRepository>();
            services.AddScoped<IDiseaseRepository, DiseaseRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IYearMasterRepository, YearMasterRepository>();
            services.AddScoped<IProjectGroupRepository, ProjectGroupRepository>();
            services.AddScoped<ITimeCostCalcsRepository, TimeCostCalcsRepository>();
            services.AddScoped<IDivisionRepository, DivisionRepository>();
            services.AddScoped<IAgencyRepository, AgencyRepository>();
            services.AddScoped<IAdditionalCostRepository, AdditionalCostRepository>();
            services.AddScoped<IAccountCategoryRepository, AccountCategoryRepository>();
            services.AddScoped<IMonthlyOutputRepository, MonthlyOutputRepository>();
            services.AddScoped<IAccountCodeRepository, AccountCodeRepository>();
            services.AddScoped<ISubAccountRepository, SubAccountRepository>();
            services.AddScoped<IStoredProcRepository, StoredProcRepository>();
            services.AddScoped<IProfitCentreRepository, ProfitCentreRepository>();
            services.AddScoped<IProfitCentreGradeRepository, ProfitCentreGradeRepository>();
            services.AddScoped<IWorkGroupGradeRepository, WorkGroupGradeRepository>();
            services.AddScoped<IWorkgroupRepository, WorkgroupRepository>();
            services.AddScoped<IWorkGroupEmployeeRepository, WorkGroupEmployeeRepository>();
            services.AddScoped<IDivisionGradeRepository, DivisionGradeRepository>();
            services.AddScoped<IProjectStaffPlanRepository, ProjectStaffPlanRepository>();
            // TRANSFORMENGINE: IGradeRepository/GradeRepository registered � Phase 5 frmMaintGrade migration
            services.AddScoped<IGradeRepository, GradeRepository>();
            services.AddScoped<IProjectGroupStaffPlanRepository, ProjectGroupStaffPlanRepository>();
            // TRANSFORMENGINE: IContributionSummaryRepository/ContributionSummaryRepository registered — Phase 5 frmTimeSellerPC migration
            services.AddScoped<IContributionSummaryRepository, ContributionSummaryRepository>();
            return services;

        }
    }
}
