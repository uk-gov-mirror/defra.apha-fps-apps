namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// One staged Staff row's reviewed classification, frozen onto
    /// fps.tblstagingprofitcentregrade's source_*/effective_*/calculated_action/
    /// validation_version columns at release time — the Staff equivalent of
    /// BulkRatesFreezeEntry, but carrying all three mutable fields rather than one rate,
    /// since Staff has no single "current rate" concept.
    /// </summary>
    public sealed record StaffFreezeEntry(
        string PcGrade,
        string CalculatedAction,
        decimal? SourcePayRate, decimal? SourceNpr, decimal? SourceOhr,
        decimal? EffectivePayRate, decimal? EffectiveNpr, decimal? EffectiveOhr);
}
