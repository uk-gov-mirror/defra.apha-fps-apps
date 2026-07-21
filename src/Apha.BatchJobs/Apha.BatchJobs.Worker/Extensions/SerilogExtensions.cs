using Microsoft.Extensions.Configuration;

namespace Apha.BatchJobs.Worker.Extensions;

/// <summary>
/// Serilog configuration helpers for the batch jobs worker.
/// Sink/format selection lives in appsettings.json / appsettings.Local.json
/// (Serilog:WriteTo) via ReadFrom.Configuration — see <see cref="ProgramExtension.ConfigureLogging"/>.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Resolves a stable prefix that appears on each log event.
    /// Exposed internally so <see cref="ProgramExtension.ConfigureLogging"/> can use the same logic.
    /// Precedence: env var BATCH_LOG_STREAM_PREFIX, config Logging:LogStreamPrefix, default value.
    /// </summary>
    internal static string ResolveLogStreamPrefix(IConfiguration? configuration)
    {
        var envPrefix = Environment.GetEnvironmentVariable("BATCH_LOG_STREAM_PREFIX");
        if (!string.IsNullOrWhiteSpace(envPrefix))
        {
            return envPrefix.Trim();
        }

        var configPrefix = configuration?["Logging:LogStreamPrefix"];
        if (!string.IsNullOrWhiteSpace(configPrefix))
        {
            return configPrefix.Trim();
        }

        return "apha-batch";
    }
}
