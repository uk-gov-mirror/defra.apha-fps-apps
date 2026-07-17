namespace Apha.BatchJobs.Domain.Entities;

/// <summary>
/// Year End approval audit columns read from fps.job_queue (see CR025).
/// configuration_json was retired under CR051 — see docs/api-to-eventbridge-ecs-batch-trigger-updated.html.
/// </summary>
public sealed record JobQueueApprovalMetadata(
    string? ApprovedBy,
    DateTime? ApprovedAtUtc,
    string? RejectedBy,
    DateTime? RejectedAtUtc,
    string? RejectionReason,
    string? TriggeredBy,
    DateTime? TriggeredAtUtc);
