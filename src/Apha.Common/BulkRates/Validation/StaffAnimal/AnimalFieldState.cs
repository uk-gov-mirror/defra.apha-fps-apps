namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// The five mutable Animal fields — all five participate in drift detection — used for
    /// both the source (live) and effective (target) state of an
    /// <see cref="AnimalValidationResult"/>. Values are already normalized (blank/
    /// null coalesced to 0/false, trimmed for text — per <see cref="StaffAnimalFieldComparer"/>)
    /// — never the raw staged value.
    /// </summary>
    public sealed record AnimalFieldState
    {
        public required decimal DailyRate { get; init; }
        public required decimal DefraDailyRate { get; init; }
        public required bool PlanByWeek { get; init; }
        public string? Species { get; init; }
        public string? SecurityLevel { get; init; }
    }
}
