using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Context;
using Apha.BatchJobs.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Infrastructure.DependencyInjection;

/// <summary>
/// Top-level composition root for Infrastructure registrations.
/// Contains no direct service registrations — delegates to focused extensions.
/// <see cref="IExecutionYearContext"/> and <see cref="ICorrelationService"/> are registered
/// here because their implementations live in the Infrastructure layer.
/// </summary>
public static class BatchInfrastructureServiceExtensions
{
    public static IServiceCollection AddBatchInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddBatchPersistence(configuration);
        services.AddGraphEmailIntegration(configuration);

        // Shared execution-context services whose implementations are in Infrastructure.
        services.AddScoped<IExecutionYearContext, ExecutionYearContext>();
        services.AddSingleton<ICorrelationService, CorrelationService>();

        return services;
    }
}
