namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>The current fps.tblanimals row for an AnimalType, as known at the moment StaffAnimalValidationContext was built.</summary>
    public sealed record LiveAnimalRow
    {
        public required string AnimalType { get; init; }
        public decimal? DailyRate { get; init; }
        public decimal? DefraDailyRate { get; init; }
        public bool PlanByWeek { get; init; }
        public string? Species { get; init; }
        public string? SecurityLevel { get; init; }
    }
}
