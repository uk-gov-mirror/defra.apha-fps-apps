using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Grouping;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Rendering;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications;

public static class MilestoneNotificationServiceExtensions
{
    public static IServiceCollection AddMilestoneNotificationJob(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MilestoneNotificationsSettings>(configuration.GetSection("MilestoneNotifications"));
        services.Configure<BatchAlertingSettings>(configuration.GetSection("BatchAlerting"));
        services.AddSingleton<IRecipientIdentityBuilder, RecipientIdentityBuilder>();
        services.AddSingleton<INotificationGroupingService, NotificationGroupingService>();
        services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddScoped<ICapsSummaryService, CapsSummaryService>();

        return services;
    }
}
