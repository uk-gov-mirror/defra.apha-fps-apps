using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Ports;
using Apha.BatchJobs.Domain.Interfaces.MabArchive;
using Apha.BatchJobs.Infrastructure.MabArchive.Repositories;
using Apha.BatchJobs.Infrastructure.MabArchive.Transactions;
using Apha.BatchJobs.Infrastructure.MabArchive.Loaders;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Infrastructure.DependencyInjection;

internal static class MabArchiveInfrastructureExtensions
{
    internal static IServiceCollection AddMabArchiveInfrastructure(
        this IServiceCollection services)
    {
        RegisterMabArchiveLoaders(services);

        services.AddScoped<IFpsTotalsRepository, FpsTotalsRepository>();
        services.AddScoped<IMabArchiveYearRepository, MabArchiveYearRepository>();
        services.AddScoped<IMabArchiveYearSelectionRepository, MabArchiveYearSelectionRepository>();
        services.AddScoped<IMabArchiveTransactionManager, MabArchiveTransactionManager>();

        return services;
    }

    /// <summary>
    /// Discovers all concrete, non-abstract <see cref="IMabArchiveLoader"/> implementations
    /// that subclass <see cref="MabArchiveExecutionLoaderBase"/>, sorted deterministically by
    /// full type name, and registers each as <see cref="IMabArchiveLoader"/>.
    /// </summary>
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
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToList();

        foreach (var loader in loaderImplementations)
        {
            services.AddScoped(typeof(IMabArchiveLoader), loader);
        }
    }
}
