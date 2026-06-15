using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Context;
using Apha.BatchJobs.Infrastructure.Repositories;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.DependencyInjection;

/// <summary>Bootstraps the DI service collection for the batch jobs worker.</summary>
public static class ServiceCollectionSetup
{
    /// <summary>Creates and returns the configured <see cref="IServiceCollection"/> for the worker host.</summary>
    /// <returns>A populated <see cref="IServiceCollection"/> ready for the host to build.</returns>
    public static IServiceCollection CreateDefaultServices(string? configurationBasePath = null)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = configurationBasePath ?? Directory.GetCurrentDirectory();

        // Build configuration from appsettings.json and environment variables
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
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

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var useInMemory = environment.Equals("Demo", StringComparison.OrdinalIgnoreCase);

        if (useInMemory)
        {
            // Demo mode: Use in-memory implementations (no database required)
            services.AddScoped<IBatchLockRepository, NoDbBatchLockRepository>();
            services.AddScoped<IJobExecutionRepository, NoDbJobExecutionRepository>();
        }
        else
        {
            // Production mode: Use PostgreSQL database
            var connectionString = config.GetConnectionString("FPSConnectionString")
                ?? throw new InvalidOperationException("Connection string 'FPSConnectionString' not found.");
            var dbCommandTimeoutSeconds = config.GetValue<int?>("BatchJobs:DbCommandTimeoutSeconds") is int v && v > 0 ? v : 30;

            services.AddDbContext<BatchJobsDbContext>(
                options =>
                {
                    options.UseNpgsql(
                        connectionString,
                        npgsqlOptions =>
                        {
                            npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorCodesToAdd: null);
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
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null);
                        npgsqlOptions.CommandTimeout(dbCommandTimeoutSeconds);
                    });
            });

            services.AddScoped<IBatchLockRepository, BatchLockRepository>();
            services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();
        }

        RegisterBatchJobs(services);
        services.AddScoped<IBatchJobFactory>(sp => new BatchJobFactory(sp));
        services.AddScoped<IJobOrchestrator, JobOrchestrator>();
        services.AddScoped<IExecutionYearContext, ExecutionYearContext>();
        services.AddSingleton(config);
        services.AddSingleton<ICorrelationService, CorrelationService>();

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

        // MABArchive Configuration and Services
        services.Configure<MabArchiveSettings>(config.GetSection("MabArchive"));
        RegisterMabArchiveLoaders(services);
        services.AddScoped<IReloadFpsTotalsService, ReloadFpsTotalsService>();
        services.AddScoped<IMyFpsYearlyDataService, MyFpsYearlyDataService>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        services.AddScoped<MabArchiveLoadOrchestrator>();

        // RecreateSummaries Configuration and Services
        services.AddScoped<IRecreateSummariesContext, RecreateSummariesContext>();
        // SQL-backed step catalogs are retired; LINQ is the only active implementation.
        services.AddScoped<IRecreateSummariesStepCatalog>(_ => new RecreateSummariesStepCatalog());
        services.AddScoped<RecreateSummariesOrchestrator>();
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

    private sealed class NoDbBatchLockRepository : IBatchLockRepository
    {
        public Task<bool> TryAcquireLockAsync(string jobName, Guid jobQueueId, int timeoutSeconds, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task ReleaseLockAsync(string jobName, Guid jobQueueId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<BatchLock?> GetActiveLockAsync(string jobName, CancellationToken cancellationToken = default)
            => Task.FromResult<BatchLock?>(null);
    }

    private sealed class NoDbJobExecutionRepository : IJobExecutionRepository
    {
        public Task<int> CreateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task UpdateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<JobExecutionRecord?> GetLastExecutionAsync(string jobName, CancellationToken cancellationToken = default)
            => Task.FromResult<JobExecutionRecord?>(null);

        public Task<JobExecutionRecord?> GetExecutionByJobExecutionIdAsync(Guid jobExecutionId, CancellationToken cancellationToken = default)
            => Task.FromResult<JobExecutionRecord?>(null);

        public Task<Guid> CreateInitiatedRecordAsync(
            string jobName,
            Guid jobExecutionId,
            string requestedBy,
            DateTime requestedAtUtc,
            RunMode runMode,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Guid.NewGuid());

        public Task EnsureJobStatusCatalogAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> TryRequestCancellationAsync(Guid jobExecutionId, string requestedBy, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<bool> UpsertCancellationRequestAsync(
            Guid jobExecutionId,
            string requestedBy,
            string? source = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<CancellationRequestRecord?> GetCancellationRequestAsync(Guid jobExecutionId, CancellationToken cancellationToken = default)
            => Task.FromResult<CancellationRequestRecord?>(null);

        public Task MarkCancellationConsumedAsync(Guid jobExecutionId, string consumedBy, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> IsCancellationRequestedAsync(Guid jobExecutionId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }

    private static void RegisterBatchJobs(IServiceCollection services)
    {
        var batchJobType = typeof(IBatchJob);
        var applicationAssembly = batchJobType.Assembly;
        var excludedJobTypes = new HashSet<Type>
        {
            // Exclude legacy/manual placeholder to keep RecreateSummaries single-source in worker orchestration.
            typeof(Apha.BatchJobs.Application.Jobs.RecreateSummaries.RecreateSummariesHandler)
        };

        var jobTypes = applicationAssembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                batchJobType.IsAssignableFrom(t) &&
                !excludedJobTypes.Contains(t))
            .ToList();

        foreach (var jobType in jobTypes)
        {
            services.AddScoped(typeof(IBatchJob), jobType);
        }
    }
}

