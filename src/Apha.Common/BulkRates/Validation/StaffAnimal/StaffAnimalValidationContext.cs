namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// IStaffAnimalValidationService's input. Everything the shared validator needs,
    /// pre-fetched by the caller — the service itself is side-effect free and does no I/O, so
    /// the same context produces the same result regardless of caller (determinism, mirroring
    /// FEC/AGRUP's ValidationContext). Lookups must be built via
    /// <see cref="StaffAnimalValidationKeys"/> so key normalization matches between context
    /// construction and validation.
    /// </summary>
    public sealed record StaffAnimalValidationContext
    {
        public required Guid JobQueueId { get; init; }
        public required int FpsYear { get; init; }

        /// <summary>Key: <see cref="StaffAnimalValidationKeys.PcGrade"/>. Every live fps.profitcentregrade row relevant to this upload.</summary>
        public required IReadOnlyDictionary<string, LiveStaffRow> LiveStaffLookup { get; init; }

        /// <summary>Key: <see cref="StaffAnimalValidationKeys.AnimalType"/>. Every live fps.tblanimals row relevant to this upload.</summary>
        public required IReadOnlyDictionary<string, LiveAnimalRow> LiveAnimalLookup { get; init; }

        public required IReadOnlyList<ValidationStaffRow> StagedStaffRows { get; init; }
        public required IReadOnlyList<ValidationAnimalRow> StagedAnimalRows { get; init; }
    }
}
