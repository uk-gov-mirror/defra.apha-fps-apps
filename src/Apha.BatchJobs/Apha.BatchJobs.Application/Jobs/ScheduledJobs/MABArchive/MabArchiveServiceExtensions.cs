using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

public static class MabArchiveServiceExtensions
{
    public static IServiceCollection AddMabArchiveJob(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MabArchiveSettings>(configuration.GetSection("MabArchive"));
        services.Configure<AwsLoggingSettings>(configuration.GetSection("AwsLogging"));

        RegisterMabArchiveLoaders(services);

        services.AddScoped<IReloadFpsTotalsService, ReloadFpsTotalsService>();
        services.AddScoped<IMyFpsYearlyDataService, MyFpsYearlyDataService>();
        services.AddScoped<IMabArchiveYearSelectionService, MabArchiveYearSelectionService>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

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
