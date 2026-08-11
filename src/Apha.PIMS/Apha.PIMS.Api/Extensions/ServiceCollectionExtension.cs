using Apha.Common.Utilities.ExcelExport;
using Apha.Common.Utilities.StateManagement;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Context;
using Apha.PIMS.DataAccess.Repository;

namespace Apha.PIMS.Api.Extensions
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
            services.AddScoped<IProjectListService, ProjectListService>();
            services.AddScoped<IProposedProjectService, ProposedProjectService>();
            services.AddScoped<IProjectDetailsService, ProjectDetailsService>();           
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IProjectYearCostsService, ProjectYearCostsService>();
            services.AddScoped<IMilestoneService, MilestoneService>();
            services.AddScoped<IRadTrackInvoiceService, RadTrackInvoiceService>();       
            services.AddScoped<IYearlyFinancialDataService, YearlyFinancialDataService>();
            services.AddScoped<IRadTrackInvoiceService, RadTrackInvoiceService>();

           
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportGroupService, ReportGroupService>();
            services.AddScoped<IReportGroupLinkService, ReportGroupLinkService>();

            
            services.AddScoped<IProjectManagerService, ProjectManagerService>();
            services.AddScoped<IProgramManagerLinkService, ProgramManagerLinkService>();
            services.AddScoped<IProfitCentreManagerLinkService, ProfitCentreManagerLinkService>();

           
            services.AddScoped<ISettingService, SettingService>();

           
            services.AddScoped<IAccessUserService, AccessUserService>();
            services.AddScoped<IAccessLevelService, AccessLevelService>();
            services.AddScoped<IAccessUserLevelService, AccessUserLevelService>();
            services.AddScoped<IAccessSystemService, AccessSystemService>();

           
            services.AddScoped<IFrequencyService, FrequencyService>();
            services.AddScoped<IReviewItemService, ReviewItemService>();

            
            services.AddScoped<IRadTrackProgService, RadTrackProgService>();

            // Risk rating lookup maintenance
            services.AddScoped<IRiskService, RiskService>();

            // Publication type lookup maintenance
            services.AddScoped<IPublicationTypeService, PublicationTypeService>();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Add your data access services here
            services.AddScoped<IFPSYearContext, FPSYearContext>();
            services.AddScoped<IProjectListRepository, ProjectListRepository>();
            services.AddScoped<IProposedProjectRepository, ProposedProjectRepository>();
            services.AddScoped<IProjectDetailsRepository, ProjectDetailsRepository>();            
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IProjectYearCostsRepository, ProjectYearCostsRepository>();
            services.AddScoped<IMilestoneRepository, MilestoneRepository>();
            services.AddScoped<IRadTrackInvoiceRepository, RadTrackInvoiceRepository>();
            services.AddScoped<IYearlyFinancialDataRepository, YearlyFinancialDataRepository>();

            
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IReportGroupRepository, ReportGroupRepository>();
            services.AddScoped<IReportGroupLinkRepository, ReportGroupLinkRepository>();

            
            services.AddScoped<IProjectManagerRepository, ProjectManagerRepository>();
            services.AddScoped<IProgramManagerLinkRepository, ProgramManagerLinkRepository>();
            services.AddScoped<IProfitCentreManagerLinkRepository, ProfitCentreManagerLinkRepository>();

            
            services.AddScoped<ISettingRepository, SettingRepository>();

            
            services.AddScoped<IAccessUserRepository, AccessUserRepository>();
            services.AddScoped<IAccessLevelRepository, AccessLevelRepository>();
            services.AddScoped<IAccessUserLevelRepository, AccessUserLevelRepository>();
            services.AddScoped<IAccessSystemRepository, AccessSystemRepository>();

            
            services.AddScoped<IFrequencyRepository, FrequencyRepository>();
            services.AddScoped<IReviewItemRepository, ReviewItemRepository>();

            
            services.AddScoped<IRadTrackProgRepository, RadTrackProgRepository>();

           
            services.AddScoped<IRiskRepository, RiskRepository>();

            
            services.AddScoped<IPublicationTypeRepository, PublicationTypeRepository>();

            return services;
        }
    }
}
