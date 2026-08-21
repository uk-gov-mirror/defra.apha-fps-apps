using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Interfaces.MilestoneUpdateNotifications;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Infrastructure.Email;
using Apha.BatchJobs.Infrastructure.MilestoneUpdateNotifications.Email;
using Apha.BatchJobs.Infrastructure.MilestoneUpdateNotifications.Repositories;
using Apha.BatchJobs.Infrastructure.MilestoneUpdateNotifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        // Email integration — IGraphEmailService is registered lazily via AddGraphEmailIntegration
        // (called from AddBatchInfrastructure). Nothing here executes at registration time;
        // Graph credentials are validated only when IEmailService is first resolved.
        // EmailTemplateRenderer is pure application logic — registered in MilestoneNotificationServiceExtensions
        services.AddScoped<IEmailService>(sp => new NonProdEmailRedirectDecorator(
            new GraphBackedEmailService(
                sp.GetRequiredService<IGraphEmailService>(),
                sp.GetRequiredService<ILogger<GraphBackedEmailService>>()),
            sp.GetRequiredService<IOptions<MilestoneNotificationsSettings>>(),
            sp.GetRequiredService<ILogger<NonProdEmailRedirectDecorator>>()));

        // EmailNotificationService (batch failure alerts) resolves IEmailService through a factory
        // so it never eagerly triggers the Graph chain above.
        services.AddScoped<Func<IEmailService>>(sp => sp.GetRequiredService<IEmailService>);

        // Write-path audit repository — uses raw Npgsql for explicit transaction boundaries.
        services.AddScoped<INotificationDeliveryRepository>(sp =>
            new NotificationDeliveryRepository(
                connectionString,
                sp.GetRequiredService<ILogger<NotificationDeliveryRepository>>()));
        // CapsSummaryService is pure application logic — registered in MilestoneNotificationServiceExtensions

        return services;
    }
}
