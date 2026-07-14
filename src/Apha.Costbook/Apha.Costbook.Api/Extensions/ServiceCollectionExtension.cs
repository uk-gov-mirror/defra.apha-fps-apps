using Apha.Common.Utilities.ExcelExport;
using Apha.Common.Utilities.StateManagement;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Context;
using Apha.Costbook.DataAccess.Repositories;

namespace Apha.Costbook.Api.Extensions
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
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IDiseaseService, DiseaseService>();
            services.AddScoped<IProgramService, ProgramService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IStaffService, StaffService>();
            services.AddScoped<IYearlyDetailsService, YearlyDetailsService>();
            services.AddScoped<IProjectSummaryService, ProjectSummaryService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IYearMasterService, YearMasterService>();
            services.AddScoped<ICapsStaffService, CapsStaffService>();
            services.AddScoped<IAccountGroupService, AccountGroupService>();
            services.AddScoped<IMaintenanceSettingsService, MaintenanceSettingsService>();
            services.AddScoped<IAccountCategoryMaintenanceService, AccountCategoryMaintenanceService>();
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IFPSYearContext, FPSYearContext>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IDiseaseRepository, DiseaseRepository>();
            services.AddScoped<IProgramRepository, ProgramRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IStaffRepository, StaffRepository>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<IProjectYearRepository, ProjectYearRepository>();
            services.AddScoped<IStaffRequirementRepository, StaffRequirementRepository>();
            services.AddScoped<ITestRequirementRepository, TestRequirementRepository>();
            services.AddScoped<IAnimalRequirementRepository, AnimalRequirementRepository>();
            services.AddScoped<IAdditionalCostRepository, AdditionalCostRepository>();
            services.AddScoped<IYearMasterRepository, YearMasterRepository>();
            services.AddScoped<ICapsStaffRepository, CapsStaffRepository>();
            services.AddScoped<IAccountGroupRepository, AccountGroupRepository>();
            services.AddScoped<IFpsAccountCategoryRepository, FpsAccountCategoryRepository>();
            return services;
        }
    }
}
