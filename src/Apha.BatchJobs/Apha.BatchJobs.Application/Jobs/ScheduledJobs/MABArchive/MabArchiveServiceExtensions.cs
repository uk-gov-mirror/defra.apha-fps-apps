using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Ports;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

public static class MabArchiveServiceExtensions
{
    public static IServiceCollection AddMabArchiveJob(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AwsLoggingSettings>(configuration.GetSection("AwsLogging"));
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        return services;
    }
}
