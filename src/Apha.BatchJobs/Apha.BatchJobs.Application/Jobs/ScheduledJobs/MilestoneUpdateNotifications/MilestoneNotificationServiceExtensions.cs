using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;
using Apha.Common.Utilities.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications;

public static class MilestoneNotificationServiceExtensions
{
    public static IServiceCollection AddMilestoneNotificationJob(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FPSConnectionString")
            ?? throw new InvalidOperationException("Connection string 'FPSConnectionString' not found.");

        services.Configure<MilestoneNotificationsSettings>(configuration.GetSection("MilestoneNotifications"));
        services.Configure<BatchAlertingSettings>(configuration.GetSection("BatchAlerting"));

        // Read-only sources, preflight, and pure grouping/identity services
        services.AddScoped<IMilestoneNotificationReadRepository, MilestoneNotificationReadRepository>();
        services.AddScoped<INotificationSettingsPreflight, NotificationSettingsPreflight>();
        services.AddScoped<IReportingYearResolver, ReportingYearResolver>();
        services.AddSingleton<IRecipientIdentityBuilder, RecipientIdentityBuilder>();
        services.AddSingleton<INotificationGroupingService, NotificationGroupingService>();

        // Email integration — IGraphEmailService is registered lazily via AddGraphEmailIntegration
        // (called from AddBatchInfrastructure). Nothing here executes at registration time;
        // Graph credentials are validated only when IEmailService is first resolved.
        services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
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

        services.AddScoped<ICapsSummaryService, CapsSummaryService>();

        return services;
    }
}
