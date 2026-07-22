using Apha.BatchJobs.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.DependencyInjection;

public static class BatchJobRegistrationExtensions
{
    /// <summary>
    /// Reflects over the Application assembly to discover all concrete, non-abstract
    /// <see cref="IBatchJob"/> implementations, sorts them deterministically by full type name,
    /// and registers each as both its concrete type and as <see cref="IBatchJob"/>.
    /// </summary>
    public static IServiceCollection RegisterBatchJobImplementations(
        this IServiceCollection services)
    {
        var batchJobType = typeof(IBatchJob);
        var applicationAssembly = batchJobType.Assembly;

        var jobTypes = applicationAssembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                batchJobType.IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

        foreach (var jobType in jobTypes)
        {
            services.AddScoped(jobType);
            services.AddScoped(typeof(IBatchJob), sp => (IBatchJob)sp.GetRequiredService(jobType));
        }

        return services;
    }
}
