namespace Apha.FPS.Core.Entities.BulkRates;

/// <summary>
/// Per-row classification frozen onto tblstagingprofitcentregrade at release time (DR-API-07).
/// </summary>
public sealed record StaffFreezeEntry(
    string PcGrade,
    string CalculatedAction,
    int ValidationVersion,
    decimal? SourcePayRate,
    decimal? SourceNpr,
    decimal? SourceOhr,
    decimal? EffectivePayRate,
    decimal? EffectiveNpr,
    decimal? EffectiveOhr);
