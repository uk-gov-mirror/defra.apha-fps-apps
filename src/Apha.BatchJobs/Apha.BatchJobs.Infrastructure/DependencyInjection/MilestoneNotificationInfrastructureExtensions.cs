using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Interfaces.MilestoneUpdateNotifications;
using Apha.BatchJobs.Infrastructure.MilestoneUpdateNotifications.Repositories;
using Apha.BatchJobs.Infrastructure.MilestoneUpdateNotifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.DependencyInjection;

internal static class MilestoneNotificationInfrastructureExtensions
{
    internal static IServiceCollection AddMilestoneNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FPSConnectionString")
            ?? throw new InvalidOperationException("Connection string 'FPSConnectionString' not found.");

        // Read-only sources, preflight, and pure grouping/identity services
        services.AddScoped<IMilestoneNotificationReadRepository, MilestoneNotificationReadRepository>();
        services.AddScoped<INotificationSettingsPreflight, NotificationSettingsPreflight>();
        services.AddScoped<IReportingYearResolver, ReportingYearResolver>();
        // RecipientIdentityBuilder, NotificationGroupingService, and EmailTemplateRenderer are pure application logic — registered in MilestoneNotificationServiceExtensions

        // Write-path audit repository — uses raw Npgsql for explicit transaction boundaries.
        services.AddScoped<INotificationDeliveryRepository>(sp =>
            new NotificationDeliveryRepository(
                connectionString,
                sp.GetRequiredService<ILogger<NotificationDeliveryRepository>>()));
        // CapsSummaryService is pure application logic — registered in MilestoneNotificationServiceExtensions

        return services;
    }
}
