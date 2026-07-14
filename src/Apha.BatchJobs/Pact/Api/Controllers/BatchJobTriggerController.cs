using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Pact.Api.Models;
using Apha.BatchJobs.Pact.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Pact.Api.Controllers;

/// <summary>
/// HTTP surface for triggering and cancelling batch jobs from the PACT API.
/// Validates the incoming request then delegates to <see cref="ITriggerDispatcher"/>.
/// </summary>
[ApiController]
[Route("api/batch-jobs")]
public sealed class BatchJobTriggerController : ControllerBase
{
    private readonly ITriggerDispatcher _dispatcher;
    private readonly ITriggerAttemptStore _attemptStore;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly IBatchLockRepository _lockRepository;
    private readonly ILogger<BatchJobTriggerController> _logger;
    private readonly IHostEnvironment _environment;

    public BatchJobTriggerController(
        ITriggerDispatcher dispatcher,
        ITriggerAttemptStore attemptStore,
        IJobExecutionRepository executionRepository,
        IBatchLockRepository lockRepository,
        ILogger<BatchJobTriggerController> logger,
        IHostEnvironment environment)
    {
        _dispatcher          = dispatcher          ?? throw new ArgumentNullException(nameof(dispatcher));
        _attemptStore        = attemptStore        ?? throw new ArgumentNullException(nameof(attemptStore));
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _lockRepository      = lockRepository      ?? throw new ArgumentNullException(nameof(lockRepository));
        _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
        _environment         = environment         ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Triggers a batch job by name.
    /// </summary>
    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger(
        [FromBody] BatchTriggerRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.RequestedBy))
            return BadRequest(new { reason = "requestedBy is required." });

        if (string.IsNullOrWhiteSpace(request.JobName))
            return BadRequest(new { reason = "jobName is required." });

        _logger.LogInformation(
            "Trigger request received | JobName={JobName} | RequestedBy={RequestedBy}",
            request.JobName, request.RequestedBy);

        var record = await _dispatcher.DispatchAsync(request, cancellationToken);
        await _attemptStore.SaveAsync(record, cancellationToken);

        return Accepted(new
        {
            jobExecutionId = record.JobExecutionId,
            jobName        = record.JobName,
            status         = record.Status,
            acceptedAtUtc  = record.AcceptedAtUtc
        });
    }

    /// <summary>
    /// Cancels a running or queued batch job by job name and execution ID.
    /// </summary>
    [HttpPost("{jobName}/cancel")]
    public async Task<IActionResult> Cancel(
        [FromRoute] string jobName,
        [FromBody] BatchCancelRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.RequestedBy))
            return BadRequest(new { reason = "requestedBy is required." });

        if (!Guid.TryParse(request.JobExecutionId, out var jobExecutionId))
            return BadRequest(new { reason = "jobExecutionId must be a valid GUID." });

        _logger.LogInformation(
            "Cancel request received | JobName={JobName} | JobExecutionId={JobExecutionId} | RequestedBy={RequestedBy}",
            jobName, jobExecutionId, request.RequestedBy);

        // Release any held lock so a re-trigger can proceed.
        await _lockRepository.ReleaseLockAsync(jobName, jobExecutionId, cancellationToken);

        return Ok(new { jobExecutionId = request.JobExecutionId, status = "CancelRequested" });
    }
}