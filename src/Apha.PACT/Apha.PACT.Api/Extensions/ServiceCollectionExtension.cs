using Apha.Common.Utilities.ExcelExport;
using Apha.Common.Utilities.StateManagement;
using Apha.PACT.Api.Context;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Context;
using Apha.PACT.DataAccess.Repository;

namespace Apha.PACT.Api.Extensions
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
            services.AddScoped<IAppStateService, AppStateService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IJobCodeService, JobCodeService>();
            services.AddScoped<ITimeCodeValidService, TimeCodeValidService>();
            services.AddScoped<IWorkGroupService, WorkGroupService>();
            services.AddScoped<IMonthService, MonthService>();
            services.AddScoped<IProjectInvoiceService, ProjectInvoiceService>();
            services.AddScoped<IProjectSubContractService, ProjectSubContractService>();
            services.AddScoped<ITestCapabilityService, TestCapabilityService>();
            services.AddScoped<ITestRequirementService, TestRequirementService>();
            services.AddScoped<ITestorProductService, TestorProductService>();
            services.AddScoped<IProjectMonthService, ProjectMonthService>();
            services.AddScoped<IProjectProfileService, ProjectProfileService>();
            services.AddScoped<IMonthlyOutputService, MonthlyOutputService>();
            services.AddScoped<ICalenderMonthService, CalenderMonthService>();
            services.AddScoped<IMonthlyTimeService, MonthlyTimeService>();
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IFpsRequestContext, FpsRequestContext>();
            services.AddScoped<ICurrentUserContext, CurrentUserContext>();
            services.AddScoped<IJobCodeRepository, JobCodeRepository>();
            services.AddScoped<ITimeCodeValidRepository, TimeCodeValidRepository>();
            services.AddScoped<IWorkGroupRepository, WorkGroupRepository>();
            services.AddScoped<IMonthRepository, MonthRepository>();
            services.AddScoped<IProjectInvoiceRepository, ProjectInvoiceRepository>();
            services.AddScoped<IProjectSubContractRepository, ProjectSubContractRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ITestCapabilityRepository, TestCapabilityRepository>();
            services.AddScoped<ITestRequirementRepository, TestRequirementRepository>();
            services.AddScoped<ITestorProductRepository, TestorProductRepository>();
            services.AddScoped<IMonthlyTimeRepository, MonthlyTimeRepository>();
            services.AddScoped<IProjectMonthRepository, ProjectMonthRepository>();
            services.AddScoped<IProjectProfileRepository, ProjectProfileRepository>();
            services.AddScoped<IMonthlyOutputRepository, MonthlyOutputRepository>();
            services.AddScoped<ICalenderMonthRepository, CalenderMonthRepository>();

            return services;
        }
    }
}
