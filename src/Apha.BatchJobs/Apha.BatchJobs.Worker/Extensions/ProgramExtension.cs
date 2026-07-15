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
    /// Configures Serilog logging. Local environment writes to console + rolling file;
    /// all other environments use structured JSON console output via configuration.
    /// </summary>
    public static void ConfigureLogging(this HostApplicationBuilder builder)
    {
        if (builder.Environment.IsEnvironment("local"))
        {
            string srvpath = builder.Configuration.GetValue<string>("LogsPath") ?? string.Empty;
            string logpath = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("local")
                ? @"Logs\Logsample.log"
                : $@"{srvpath}\Logsample.log";

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "FPSBatchJobs")
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .Enrich.WithProperty("LogStreamPrefix", SerilogExtensions.ResolveLogStreamPrefix(builder.Configuration))
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{LogStreamPrefix}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: logpath,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{LogStreamPrefix}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateLogger();
        }
        else
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .UseStructuredConsoleLogging(builder.Configuration)
                .CreateLogger();
        }

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
