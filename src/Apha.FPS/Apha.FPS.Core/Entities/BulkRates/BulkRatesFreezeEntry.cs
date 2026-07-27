namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// DR-API-07: one row's reviewed classification, frozen onto the FEC/AGRUP staging
    /// tables' calculated_action/effective_new_rate/source_current_rate columns (CR056) at
    /// release time, so the worker's revalidation (DR-WK-04 §5.2) can compare its re-derived
    /// result against exactly what the approver saw. Buyer is null for FEC rows.
    /// </summary>
    public sealed record BulkRatesFreezeEntry(
        string TestCode,
        string? Buyer,
        string CalculatedAction,
        decimal? EffectiveNewRate,
        decimal? SourceCurrentRate);
}
