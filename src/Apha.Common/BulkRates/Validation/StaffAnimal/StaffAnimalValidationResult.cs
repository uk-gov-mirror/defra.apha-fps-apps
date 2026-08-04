namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>IStaffAnimalValidationService.Validate's return value: one classification per staged Staff/Animal row.</summary>
    public sealed record StaffAnimalValidationResult
    {
        public required IReadOnlyList<StaffValidationResult> StaffResults { get; init; }
        public required IReadOnlyList<AnimalValidationResult> AnimalResults { get; init; }
    }
}
