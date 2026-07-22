using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Context;
using Apha.BatchJobs.Infrastructure.Repositories;
using Apha.BatchJobs.Infrastructure.Repositories.BulkRates;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;
using Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Services;
using Apha.BatchJobs.Infrastructure.Services.BulkRates;
using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.Email;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace Apha.BatchJobs.Application.DependencyInjection;

/// <summary>Bootstraps the DI service collection for the batch jobs worker.</summary>
public static class ServiceCollectionSetup
{
    /// <summary>Creates and returns the configured <see cref="IServiceCollection"/> for the worker host.</summary>
    /// <returns>A populated <see cref="IServiceCollection"/> ready for the host to build.</returns>
    public static IServiceCollection CreateDefaultServices(string? configurationBasePath = null)
    {
        var environment = EnvironmentResolver.GetEnvironmentName("Development");
        var basePath = configurationBasePath ?? Directory.GetCurrentDirectory();

        // Build configuration from appsettings.json and environment variables
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        ConfigureBatchJobServices(services, config);

        return services;
    }

    /// <summary>Registers all batch job services, repositories and job handlers into the provided service collection.</summary>
    public static void ConfigureBatchJobServices(IServiceCollection services, IConfiguration config)
    {
        services.Configure<BatchJobSettings>(config.GetSection("BatchJobs"));
        services.AddLogging();

        var connectionString = config.GetConnectionString("FPSConnectionString")
            ?? throw new InvalidOperationException("Connection string 'FPSConnectionString' not found.");
        var dbCommandTimeoutSeconds = config.GetValue<int?>("BatchJobs:DbCommandTimeoutSeconds") is int v && v >= 0 ? v : 0;

        services.AddDbContext<BatchJobsDbContext>(
            options =>
            {
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.ExecutionStrategy(ctx =>
                            new BatchJobsRetryStrategy(ctx, maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10)));
                        npgsqlOptions.CommandTimeout(dbCommandTimeoutSeconds);
                    });
            },
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        // Some jobs resolve DbContext instances through IDbContextFactory for per-operation scopes.
        services.AddDbContextFactory<BatchJobsDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.ExecutionStrategy(ctx =>
                        new BatchJobsRetryStrategy(ctx, maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10)));
                    npgsqlOptions.CommandTimeout(dbCommandTimeoutSeconds);
                });
        });

        services.AddScoped<IBatchLockRepository, BatchLockRepository>();
        services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();

        RegisterBatchJobs(services);
        services.AddScoped<IBatchJobFactory>(sp => new BatchJobFactory(sp));
        services.AddScoped<IJobOrchestrator, JobOrchestrator>();
        services.AddScoped<IExecutionYearContext, ExecutionYearContext>();
        services.AddSingleton(config);
        services.AddSingleton<ICorrelationService, CorrelationService>();
        services.AddScoped<IYearEndDataSetupStep, ValidateYearEndContextStep>();
        services.AddScoped<IYearEndDataSetupStep, ValidateYearScopedSchemaStep>();
        services.AddScoped<IYearEndDataSetupStep, CreatePlannedYearStep>();
        services.AddScoped<IYearEndDataSetupStep, CopyFpsYearScopedTablesStep>();
        services.AddScoped<IYearEndDataSetupStep, CopyMabArchiveYearScopedTablesStep>();
        services.AddScoped<IYearEndDataSetupStep, PeriodSetupStep>();
        services.AddScoped<IYearEndDataSetupStep, ProjectFinancialResetStep>();
        services.AddScoped<IYearEndDataSetupStep, ConfiguredPlanningResetStep>();
        services.AddScoped<IYearEndDataSetupStep, InactiveEmployeeCleanupStep>();
        services.AddScoped<IYearEndDataSetupStep, TargetYearEmptyTablesStep>();
        services.AddScoped<IYearEndDataSetupStep, FinalValidationStep>();
        services.AddScoped<IYearEndDataSetupService, YearEndDataSetupService>();
        services.AddScoped<IYearEndCutoverService, YearEndCutoverService>();

        // BulkRates — repositories and per-stream services
        services.AddScoped<IBulkRatesRepository, BulkRatesRepository>();
        services.AddScoped<IBulkTestRatesService, BulkTestRatesService>();
        services.AddScoped<IBulkStaffRatesService, BulkStaffRatesService>();
        services.AddScoped<IBulkAnimalRatesService, BulkAnimalRatesService>();

        services.AddScoped<Func<Func<Task>, Task>>(sp =>
        {
            return async action =>
            {
                var dbContext = sp.GetService<BatchJobsDbContext>();
                if (dbContext is null)
                {
                    await action();
                    return;
                }

                var executionStrategy = dbContext.Database.CreateExecutionStrategy();
                await executionStrategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync();
                    await action();
                    await transaction.CommitAsync();
                });
            };
        });

        // Shared, job-agnostic settings — not nested under any single job's config.
        services.Configure<BatchAlertingSettings>(config.GetSection("BatchAlerting"));
        services.Configure<AwsLoggingSettings>(config.GetSection("AwsLogging"));

        // MABArchive Configuration and Services
        services.Configure<MabArchiveSettings>(config.GetSection("MabArchive"));
        RegisterMabArchiveLoaders(services);
        services.AddScoped<IReloadFpsTotalsService, ReloadFpsTotalsService>();
        services.AddScoped<IMyFpsYearlyDataService, MyFpsYearlyDataService>();
        services.AddScoped<IMabArchiveYearSelectionService, MabArchiveYearSelectionService>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        services.AddScoped<MabArchiveLoadOrchestrator>();

        // MilestoneUpdateNotifications — read-only sources, preflight, and pure grouping/identity services
        services.AddScoped<IMilestoneNotificationReadRepository, MilestoneNotificationReadRepository>();
        services.AddScoped<INotificationSettingsPreflight, NotificationSettingsPreflight>();
        services.AddScoped<IReportingYearResolver, ReportingYearResolver>();
        services.AddSingleton<IRecipientIdentityBuilder, RecipientIdentityBuilder>();
        services.AddSingleton<INotificationGroupingService, NotificationGroupingService>();

        // MilestoneUpdateNotifications — email integration (plan §21 step 4). GraphServiceClient and
        // IGraphEmailService are registered with lazy factories: nothing below runs until something
        // actually resolves IEmailService, so a Worker host with no GraphEmailSettings configured yet
        // (true everywhere right now — no live send has been authorised) still starts and runs every
        // other job normally. This deliberately differs from Apha.PACT's eager GetRequiredSection
        // validation at registration time, which would abort the whole host at startup instead of only
        // failing when this job actually tries to send.
        services.Configure<MilestoneNotificationsSettings>(config.GetSection("MilestoneNotifications"));
        services.AddSingleton(_ => CreateGraphServiceClient(config));
        services.AddSingleton<IGraphEmailService, GraphEmailService>();
        services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddScoped<IEmailService>(sp => new NonProdEmailRedirectDecorator(
            new GraphBackedEmailService(
                sp.GetRequiredService<IGraphEmailService>(),
                sp.GetRequiredService<ILogger<GraphBackedEmailService>>()),
            sp.GetRequiredService<IOptions<MilestoneNotificationsSettings>>(),
            sp.GetRequiredService<ILogger<NonProdEmailRedirectDecorator>>()));

        // EmailNotificationService (used for batch failure alerts, see above) depends on this
        // factory rather than IEmailService directly, so constructing it never eagerly triggers
        // the GraphServiceClient chain above — only actually sending a notification does.
        services.AddScoped<Func<IEmailService>>(sp => sp.GetRequiredService<IEmailService>);

        // MilestoneUpdateNotifications — write-path audit repository and CAPS summary service.
        // NotificationDeliveryRepository uses raw Npgsql (not EF) for explicit transaction
        // boundaries — connection string is captured from the already-resolved local variable.
        services.AddScoped<INotificationDeliveryRepository>(sp =>
            new NotificationDeliveryRepository(
                connectionString,
                sp.GetRequiredService<ILogger<NotificationDeliveryRepository>>()));
        services.AddScoped<ICapsSummaryService, CapsSummaryService>();

        // RecreateSummaries Configuration and Services
        services.AddScoped<IRecreateSummariesContext, RecreateSummariesContext>();
        // SQL-backed step catalogs are retired; LINQ is the only active implementation.
        services.AddScoped<IRecreateSummariesStepCatalog>(sp => new RecreateSummariesStepCatalog(sp.GetRequiredService<ILoggerFactory>()));
    }

    private static readonly string[] GraphEmailScopes = ["https://graph.microsoft.com/.default"];

    /// <summary>
    /// Builds the <see cref="GraphServiceClient"/> used for milestone notification email.
    /// Only invoked when something actually resolves it (see the registration comment above) —
    /// validation failures surface as an <see cref="InvalidOperationException"/> at that point,
    /// not at Worker host startup.
    /// </summary>
    private static GraphServiceClient CreateGraphServiceClient(IConfiguration config)
    {
        var graphSettings = config.GetSection("GraphEmailSettings").Get<GraphEmailSettings>()
            ?? throw new InvalidOperationException("GraphEmailSettings configuration section is missing or could not be bound.");

        if (string.IsNullOrWhiteSpace(graphSettings.TenantId))
            throw new InvalidOperationException("GraphEmailSettings:TenantId is required but was not configured.");
        if (string.IsNullOrWhiteSpace(graphSettings.ClientId))
            throw new InvalidOperationException("GraphEmailSettings:ClientId is required but was not configured.");
        if (string.IsNullOrWhiteSpace(graphSettings.ClientSecret))
            throw new InvalidOperationException("GraphEmailSettings:ClientSecret is required but was not configured.");

        var credential = new ClientSecretCredential(graphSettings.TenantId, graphSettings.ClientId, graphSettings.ClientSecret);
        return new GraphServiceClient(credential, GraphEmailScopes);
    }

    private static void RegisterMabArchiveLoaders(IServiceCollection services)
    {
        var loaderType = typeof(IMabArchiveLoader);
        var loaderAssembly = typeof(MyTlkpProgramLoader).Assembly;

        var loaderImplementations = loaderAssembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                loaderType.IsAssignableFrom(t) &&
                t.IsSubclassOf(typeof(MabArchiveExecutionLoaderBase)))
            .ToList();

        foreach (var loader in loaderImplementations)
        {
            services.AddScoped(typeof(IMabArchiveLoader), loader);
        }
    }

    private static void RegisterBatchJobs(IServiceCollection services)
    {
        var batchJobType = typeof(IBatchJob);
        var applicationAssembly = batchJobType.Assembly;

        var jobTypes = applicationAssembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                batchJobType.IsAssignableFrom(t))
            .ToList();

        foreach (var jobType in jobTypes)
        {
            services.AddScoped(jobType);
            services.AddScoped(typeof(IBatchJob), sp => (IBatchJob)sp.GetRequiredService(jobType));
        }
    }
}

