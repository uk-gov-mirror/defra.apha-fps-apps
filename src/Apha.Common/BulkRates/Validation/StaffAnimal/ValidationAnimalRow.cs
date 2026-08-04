namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// A staged Animal row, in a shape neither Apha.FPS.Core.Entities.BulkRates.AnimalStagingRow
    /// nor Apha.BatchJobs's own staging types can express directly — Apha.Common cannot see
    /// either project's staging types, so each caller maps its native row into this before
    /// calling IStaffAnimalValidationService, and maps the result back out.
    /// </summary>
    public sealed record ValidationAnimalRow
    {
        public required string AnimalType { get; init; }
        public decimal? DailyRate { get; init; }
        public decimal? DefraDailyRate { get; init; }
        public bool? PlanByWeek { get; init; }
        public string? Species { get; init; }
        public string? SecurityLevel { get; init; }

        /// <summary>1-based worksheet row number (row 1 is the header), for finding attribution.</summary>
        public required int SourceRow { get; init; }
    }
}
