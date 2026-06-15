using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Fps.Api.Models;
using Apha.BatchJobs.Fps.Api.Policy;
using Apha.BatchJobs.Fps.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Apha.BatchJobs.Fps.Api.Controllers;

[ApiController]
[Route("api/v{version}/batch-jobs")]
public sealed class BatchJobTriggerController : ControllerBase
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IJobExecutionRepository? _executionRepository;
    private readonly ILogger<BatchJobTriggerController> _logger;

    public BatchJobTriggerController(
        IEventPublisher eventPublisher,
        ILogger<BatchJobTriggerController> logger,
        IJobExecutionRepository? executionRepository = null)
    {
        _eventPublisher = eventPublisher;
        _executionRepository = executionRepository;
        _logger = logger;
    }

    [HttpGet("catalog")]
    public IActionResult GetCatalog()
    {
        var catalog = BatchJobRoutingPolicy
            .GetCatalog()
            .Select(route => new
            {
                route.JobName,
                route.Description,
                route.RouteKind,
                CanTriggerFromThisApi = route.RouteKind is JobRouteKind.FpsApi or JobRouteKind.Neutral
            });

        return Ok(new { api = "fps.api", jobs = catalog });
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger([FromBody] BatchTriggerRequest request, CancellationToken cancellationToken)
    {
        if (!BatchJobRoutingPolicy.CanTriggerFromSource(
                request.JobName,
                TriggerApiSource.Fps,
                out var normalizedJobName,
                out var reason))
        {
            return Conflict(new
            {
                accepted = false,
                source = "fps.api",
                jobName = request.JobName,
                reason
            });
        }

        var jobExecutionId = Guid.NewGuid().ToString("N");
        var acceptedAtUtc = DateTime.UtcNow;
        var requestedBy = string.IsNullOrWhiteSpace(request.RequestedBy) ? "fps.api@local" : request.RequestedBy;
        var parametersJson = string.IsNullOrWhiteSpace(request.ParametersJson) ? null : request.ParametersJson;

        if (string.Equals(normalizedJobName, "RecreateSummaries", StringComparison.OrdinalIgnoreCase))
        {
            if (!TryParseParametersJson(parametersJson, out var parsedParameters, out var parametersError)
                || !parsedParameters.TryGetValue("month", out var monthValue)
                || !Regex.IsMatch(monthValue, "^\\d{4}-(0[1-9]|1[0-2])$"))
            {
                return BadRequest(new { accepted = false, reason = parametersError ?? "parametersJson.month is required and must use YYYY-MM for RecreateSummaries." });
            }
        }
        else if (!string.IsNullOrWhiteSpace(parametersJson)
                 && !TryParseParametersJson(parametersJson, out _, out var parametersError))
        {
            return BadRequest(new { accepted = false, reason = parametersError });
        }

        // Create Initiated record in database before publishing event
        Guid jobQueueId = Guid.Empty;
        if (_executionRepository != null)
        {
            try
            {
                jobQueueId = await _executionRepository.CreateInitiatedRecordAsync(
                    normalizedJobName,
                    Guid.Parse(jobExecutionId),
                    requestedBy,
                    acceptedAtUtc,
                    Apha.BatchJobs.Domain.Enums.RunMode.Manual,
                    cancellationToken);
                _logger.LogInformation(
                    "✓ Initiated record created in database | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId}",
                    normalizedJobName, jobExecutionId, jobQueueId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "✗ Failed to create Initiated record for job {JobName} | JobExecutionId={JobExecutionId}", normalizedJobName, jobExecutionId);
                return StatusCode(500, new { accepted = false, reason = "Failed to record job initiation in database." });
            }
        }

        var eventId = await _eventPublisher.PublishAsync(
            new BatchTriggerEventDetail(
                jobExecutionId,
                normalizedJobName,
                "Manual",
                requestedBy,
                acceptedAtUtc,
                parametersJson),
            cancellationToken);

        _logger.LogInformation(
            "FPS API trigger accepted | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | EventId={EventId}",
            normalizedJobName,
            jobExecutionId,
            jobQueueId,
            eventId);

        return Accepted(new
        {
            accepted = true,
            source = "fps.api",
            jobName = normalizedJobName,
            jobExecutionId,
            eventId,
            status = "TriggerAccepted",
            acceptedAtUtc,
            parametersJson,
            message = "Event accepted for EventBridge dispatch."
        });
    }

    private static bool TryParseParametersJson(string? parametersJson, out Dictionary<string, string> values, out string? error)
    {
        values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        error = null;

        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            error = "parametersJson is required.";
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(parametersJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                error = "parametersJson must be a JSON object.";
                return false;
            }

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                values[property.Name] = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString() ?? string.Empty
                    : property.Value.GetRawText();
            }

            return true;
        }
        catch (JsonException)
        {
            error = "parametersJson must be valid JSON.";
            return false;
        }
    }
}