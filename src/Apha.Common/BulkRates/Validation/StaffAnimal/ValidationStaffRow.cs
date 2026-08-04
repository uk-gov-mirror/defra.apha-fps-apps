namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// A staged Staff row, in a shape neither Apha.FPS.Core.Entities.BulkRates.StaffStagingRow
    /// nor Apha.BatchJobs's own staging types can express directly — Apha.Common cannot see
    /// either project's staging types, so each caller maps its native row into this before
    /// calling IStaffAnimalValidationService, and maps the result back out.
    /// </summary>
    public sealed record ValidationStaffRow
    {
        public required string PcGrade { get; init; }
        public decimal? PayRate { get; init; }
        public decimal? Npr { get; init; }
        public decimal? Ohr { get; init; }

        /// <summary>1-based worksheet row number (row 1 is the header), for finding attribution.</summary>
        public required int SourceRow { get; init; }
    }
}
