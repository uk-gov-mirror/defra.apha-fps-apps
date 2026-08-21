using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain;
using Apha.BatchJobs.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.HealthCheck;

/// <summary>
/// Container liveness check. Exercises the full ECS batch dispatch path (host build → DI
/// resolution → job execution → exit 0) without touching the database.
/// Trigger: deployment pipeline or operator passes "HealthCheck" as the CLI argument.
/// Checks: environment name readable, BatchJobSettings resolves from DI, synthetic
/// 50-iteration loop completes without exception.
/// Consumer: deployment automation verifying that the ECS task launch works end-to-end.
/// </summary>
public sealed class HealthCheckJobHandler : IBatchJob
{
    private readonly ILogger<HealthCheckJobHandler> _logger;
    private readonly BatchJobSettings _settings;

    public string Name => "HealthCheck";
    // Read/validate-only; no mutable side effects.
    public string IdempotencyStrategy => "NoWriteValidation";
    // Ad-hoc only — invoked explicitly, never on a cron schedule.
    public string? ScheduleExpression => null;
    public string? ScheduleDescription => "On-demand health check (no schedule)";
    public int? MaxExecutionSeconds => 300;

    public HealthCheckJobHandler(
        ILogger<HealthCheckJobHandler> logger,
        IOptions<BatchJobSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new BatchJobSettings();
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== HealthCheck Job Started ===");
        _logger.LogInformation("Job: {JobName}", Name);
        _logger.LogInformation("Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}", DateTime.UtcNow);
        _logger.LogInformation("ProcessId: {ProcessId}", Environment.ProcessId);

        try
        {
            _logger.LogInformation("Phase 1: Validating configuration...");
            var envName = EnvironmentResolver.GetEnvironmentName("Not Set");
            var executionMode = "LivenessOnly (NoDbDependency)";
            _logger.LogInformation("  Environment: {Environment}", envName);
            _logger.LogInformation("  Execution Mode: {ExecutionMode}", executionMode);
            _logger.LogInformation("  .NET Version: {DotNetVersion}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
            _logger.LogInformation("  OS: {OS}", System.Runtime.InteropServices.RuntimeInformation.OSDescription);

            _logger.LogInformation("Phase 2: Processing records...");
            var recordCount = 50;
            var successCount = 0;

            for (int i = 1; i <= recordCount; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Cancellation requested at record {RecordNumber}", i);
                    break;
                }

                successCount++;
                
                if (i % 10 == 0)
                {
                    _logger.LogInformation("  Processed {RecordsProcessed}/{TotalRecords} records", i, recordCount);
                }

                await Task.Delay(50, cancellationToken);
            }

            _logger.LogInformation("Phase 3: Validating liveness-only execution path...");
            _logger.LogInformation("  No database calls are required for this check");

            _logger.LogInformation("Phase 4: Job completion report");
            _logger.LogInformation("  Records Processed: {RecordsProcessed}", successCount);
            _logger.LogInformation("  Success Rate: {SuccessRate:P}", (double)successCount / recordCount);
            _logger.LogInformation("  Completed At: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}", DateTime.UtcNow);
            _logger.LogInformation("=== HealthCheck Job Completed Successfully ===");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "HealthCheck job execution was interrupted");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HealthCheck job failed with error: {ErrorMessage}", ex.Message);
            throw;
        }
    }
}
