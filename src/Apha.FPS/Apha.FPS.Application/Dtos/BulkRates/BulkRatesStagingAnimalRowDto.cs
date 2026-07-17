namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// A single Animal staging row for the "Animal Data (Staging)" grid — diffed against live
    /// fps.tblanimals. Status is "Updated" (a field differs, will be applied) or "Not Found"
    /// (AnimalType doesn't exist for this FPS year — the worker skips these rows, it never
    /// inserts). Unlike FEC/AGRUP, Animal rates are update-only: there is no
    /// "Inserted"/"Deleted" status because the worker can do neither.
    /// </summary>
    public class BulkRatesStagingAnimalRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public string? Species { get; set; }
        public string? SecurityLevel { get; set; }
        public decimal? DailyRate { get; set; }
        public decimal? DailyRateNew { get; set; }
        public decimal? DefraDailyRate { get; set; }
        public decimal? DefraDailyRateNew { get; set; }
        public bool? PlanByWeek { get; set; }
    }
}
