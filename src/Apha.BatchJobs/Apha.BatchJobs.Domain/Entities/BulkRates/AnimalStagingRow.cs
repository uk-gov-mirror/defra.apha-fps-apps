namespace Apha.BatchJobs.Domain.Entities.BulkRates;

/// <summary>
/// Represents a row from fps.tblstaginganimals for a specific request.
/// Maps to Animal rate staging data uploaded by the initiator.
/// </summary>
public sealed record AnimalStagingRow(
    Guid JobQueueId,
    string AnimalType,
    string? Species,
    string? SecurityLevel,
    decimal? DailyRate,
    decimal? DefraDailyRate,
    bool? PlanByWeek,
    // Frozen at release time,
    // compared against the worker's re-derived result (§6 drift check). Null until release.
    string? CalculatedAction = null,
    decimal? SourceDailyRate = null,
    decimal? SourceDefraDailyRate = null,
    bool? SourcePlanByWeek = null,
    string? SourceSpecies = null,
    string? SourceSecurityLevel = null,
    decimal? EffectiveDailyRate = null,
    decimal? EffectiveDefraDailyRate = null,
    bool? EffectivePlanByWeek = null,
    string? EffectiveSpecies = null,
    string? EffectiveSecurityLevel = null,
    int? ValidationVersion = null);
