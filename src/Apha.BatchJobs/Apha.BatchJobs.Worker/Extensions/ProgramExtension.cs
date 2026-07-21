using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Apha.BatchJobs.Worker.Extensions;

/// <summary>
/// Configures worker services and logging using the shared BatchJobs registration pattern.
/// </summary>
public static class ProgramExtension
{
    /// <summary>
    /// Configures Serilog logging. Sinks and formats are sourced entirely from configuration
    /// (Serilog:WriteTo in appsettings.json / appsettings.Local.json) so switching between the
    /// readable local console+file setup and structured JSON console output for
    /// deployed environments requires no code changes — just which appsettings file layers in.
    /// </summary>
    public static void ConfigureLogging(this HostApplicationBuilder builder)
    {
        Serilog.Debugging.SelfLog.Enable(Console.Error);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "FPSBatchJobs")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .Enrich.WithProperty("LogStreamPrefix", SerilogExtensions.ResolveLogStreamPrefix(builder.Configuration))
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
    }

    /// <summary>
    /// Registers worker dependencies for the batch jobs host.
    /// </summary>
    public static void ConfigureServices(this HostApplicationBuilder builder)
    {
        Apha.BatchJobs.Application.DependencyInjection.ServiceCollectionSetup
            .ConfigureBatchJobServices(builder.Services, builder.Configuration);
    }
}
