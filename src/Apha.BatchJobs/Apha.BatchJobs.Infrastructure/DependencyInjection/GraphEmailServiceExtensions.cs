using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.Email;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;

namespace Apha.BatchJobs.Infrastructure.DependencyInjection;

public static class GraphEmailServiceExtensions
{
    private static readonly string[] GraphEmailScopes = ["https://graph.microsoft.com/.default"];

    /// <summary>
    /// Registers <see cref="GraphServiceClient"/> and <see cref="IGraphEmailService"/> as
    /// singletons with lazy factories. Nothing here executes at registration time — Graph
    /// credentials are validated only when something first resolves one of these services.
    /// Worker host startup and HealthCheck never trigger this factory.
    /// </summary>
    public static IServiceCollection AddGraphEmailIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(_ => CreateGraphServiceClient(configuration));
        services.AddSingleton<IGraphEmailService, GraphEmailService>();

        return services;
    }

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

        var credential = new ClientSecretCredential(
            graphSettings.TenantId,
            graphSettings.ClientId,
            graphSettings.ClientSecret);

        return new GraphServiceClient(credential, GraphEmailScopes);
    }
}
