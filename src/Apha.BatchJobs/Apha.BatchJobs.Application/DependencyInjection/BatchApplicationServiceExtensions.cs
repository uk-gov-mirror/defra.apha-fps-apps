using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.DependencyInjection;

/// <summary>
/// Registers application-layer orchestration services.
/// Contains no references to Infrastructure namespaces — only Application, Domain,
/// and framework abstractions.
/// </summary>
public static class BatchApplicationServiceExtensions
{
    public static IServiceCollection AddBatchApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BatchJobSettings>(configuration.GetSection("BatchJobs"));

        services.AddScoped<IBatchJobFactory>(sp => new BatchJobFactory(sp));
        services.AddScoped<IJobOrchestrator, JobOrchestrator>();

        return services;
    }
}
