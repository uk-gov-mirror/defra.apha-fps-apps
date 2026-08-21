using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Infrastructure.Email;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    /// <summary>
    /// Registers <see cref="IEmailService"/> (Graph-backed, with non-prod redirect) and its
    /// <see cref="Func{IEmailService}"/> factory. Shared by all jobs — MABArchive resolves the
    /// factory lazily so Graph credentials are never eagerly validated at startup.
    /// </summary>
    public static IServiceCollection AddEmailService(this IServiceCollection services)
    {
        // Graph credentials are validated only when IEmailService is first resolved.
        services.AddScoped<IEmailService>(sp => new NonProdEmailRedirectDecorator(
            new GraphBackedEmailService(
                sp.GetRequiredService<IGraphEmailService>(),
                sp.GetRequiredService<ILogger<GraphBackedEmailService>>()),
            sp.GetRequiredService<IOptions<EmailDeliverySettings>>(),
            sp.GetRequiredService<ILogger<NonProdEmailRedirectDecorator>>()));

        // Func<IEmailService> lets MABArchive resolve IEmailService lazily without triggering Graph.
        services.AddScoped<Func<IEmailService>>(sp => sp.GetRequiredService<IEmailService>);

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
