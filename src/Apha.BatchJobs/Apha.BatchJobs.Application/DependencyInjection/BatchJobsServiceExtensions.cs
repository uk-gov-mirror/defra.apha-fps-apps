using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.DependencyInjection;

/// <summary>
/// Top-level composition root for all batch job services.
/// This method must remain short and contain no direct service registrations — composition only.
/// </summary>
public static class BatchJobsServiceExtensions
{
    public static IServiceCollection AddBatchJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddBatchApplicationServices(configuration);
        services.AddBatchInfrastructure(configuration);

        services.AddYearEndJob();
        services.AddBulkRatesJobs();
        services.AddMabArchiveJob(configuration);
        services.AddMilestoneNotificationJob(configuration);
        services.AddRecreateSummariesJob();

        services.RegisterBatchJobImplementations();

        return services;
    }
}
