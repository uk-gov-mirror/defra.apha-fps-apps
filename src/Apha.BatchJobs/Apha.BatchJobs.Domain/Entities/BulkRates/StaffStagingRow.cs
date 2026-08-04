namespace Apha.BatchJobs.Domain.Entities.BulkRates;

/// <summary>
/// Represents a row from fps.tblstagingprofitcentregrade for a specific request.
/// Maps to Staff profit-centre grade staging data uploaded by the initiator.
/// </summary>
public sealed record StaffStagingRow(
    Guid JobQueueId,
    string PcGrade,
    decimal? PayRate,
    decimal? Npr,
    decimal? Ohr,
    // Frozen at release time,
    // compared against the worker's re-derived result (§6 drift check). Null until release.
    string? CalculatedAction = null,
    decimal? SourcePayRate = null,
    decimal? SourceNpr = null,
    decimal? SourceOhr = null,
    decimal? EffectivePayRate = null,
    decimal? EffectiveNpr = null,
    decimal? EffectiveOhr = null,
    int? ValidationVersion = null);
