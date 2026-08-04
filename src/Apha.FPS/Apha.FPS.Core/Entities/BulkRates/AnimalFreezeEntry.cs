namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// One staged Animal row's reviewed classification, frozen onto
    /// fps.tblstaginganimals's source_*/effective_*/calculated_action/validation_version
    /// columns at release time — the Animal equivalent of BulkRatesFreezeEntry,
    /// carrying all five mutable fields — all five participate in drift
    /// detection.
    /// </summary>
    public sealed record AnimalFreezeEntry(
        string AnimalType,
        string CalculatedAction,
        decimal? SourceDailyRate, decimal? SourceDefraDailyRate, bool? SourcePlanByWeek, string? SourceSpecies, string? SourceSecurityLevel,
        decimal? EffectiveDailyRate, decimal? EffectiveDefraDailyRate, bool? EffectivePlanByWeek, string? EffectiveSpecies, string? EffectiveSecurityLevel);
}
