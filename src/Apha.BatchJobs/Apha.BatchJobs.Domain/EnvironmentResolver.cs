namespace Apha.BatchJobs.Domain;

/// <summary>
/// Resolves the effective environment name for the batch jobs worker.
/// Host.CreateApplicationBuilder (used by Apha.BatchJobs.Worker) only recognizes
/// DOTNET_ENVIRONMENT for IHostEnvironment.EnvironmentName — ASPNETCORE_ENVIRONMENT is an
/// ASP.NET Core / WebApplicationBuilder convention this generic host does not read. Checking
/// DOTNET_ENVIRONMENT first keeps ad-hoc environment checks scattered across the app in sync
/// with builder.Environment.EnvironmentName; ASPNETCORE_ENVIRONMENT remains a fallback since
/// it is already set explicitly in deployment (see Dockerfile).
/// </summary>
public static class EnvironmentResolver
{
    public static string GetEnvironmentName(string fallback) =>
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? fallback;
}
