using Apha.BatchJobs.Infrastructure.Resilience;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Operational.Repositories;
using Apha.BatchJobs.Infrastructure.YearEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Infrastructure.DependencyInjection;

public static class BatchPersistenceServiceExtensions
{
    public static IServiceCollection AddBatchPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FPSConnectionString")
            ?? throw new InvalidOperationException("Connection string 'FPSConnectionString' not found.");

        var dbCommandTimeoutSeconds =
            configuration.GetValue<int?>("BatchJobs:DbCommandTimeoutSeconds") is int v && v >= 0 ? v : 0;

        services.AddDbContext<BatchJobsDbContext>(
            options => ConfigureNpgsql(options, connectionString, dbCommandTimeoutSeconds),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddDbContextFactory<BatchJobsDbContext>(
            options => ConfigureNpgsql(options, connectionString, dbCommandTimeoutSeconds));

        services.AddScoped<IBatchLockRepository, BatchLockRepository>();
        services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();
        services.AddScoped<IYearEndCutoverRepository, YearEndCutoverRepository>();
        services.AddScoped<IYearEndDataSetupRepository, YearEndDataSetupRepository>();

        return services;
    }

    private static void ConfigureNpgsql(
        DbContextOptionsBuilder options,
        string connectionString,
        int commandTimeoutSeconds)
    {
        options.UseNpgsql(
            connectionString,
            npgsql =>
            {
                npgsql.ExecutionStrategy(ctx =>
                    new BatchJobsRetryStrategy(ctx, maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10)));
                npgsql.CommandTimeout(commandTimeoutSeconds);
            });
    }
}
