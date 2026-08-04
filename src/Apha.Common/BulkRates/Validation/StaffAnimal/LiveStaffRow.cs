namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>The current fps.profitcentregrade row for a PcGrade, as known at the moment StaffAnimalValidationContext was built.</summary>
    public sealed record LiveStaffRow
    {
        public required string PcGrade { get; init; }
        public decimal? PayRate { get; init; }
        public decimal? Npr { get; init; }
        public decimal? Ohr { get; init; }
    }
}
