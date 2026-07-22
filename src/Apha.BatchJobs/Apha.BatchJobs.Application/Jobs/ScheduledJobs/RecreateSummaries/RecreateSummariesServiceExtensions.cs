using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Context;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;

public static class RecreateSummariesServiceExtensions
{
    public static IServiceCollection AddRecreateSummariesJob(
        this IServiceCollection services)
    {
        services.AddScoped<IRecreateSummariesContext, RecreateSummariesContext>();
        // SQL-backed step catalogs are retired; LINQ is the only active implementation.
        services.AddScoped<IRecreateSummariesStepCatalog>(sp =>
            new RecreateSummariesStepCatalog(sp.GetRequiredService<ILoggerFactory>()));

        return services;
    }
}
