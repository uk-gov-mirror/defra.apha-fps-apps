namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// A single Staff staging row for the "Staff Data (Staging)" grid — diffed against live
    /// fps.profitcentregrade. Status is "Updated" (rate differs, will be applied) or
    /// "Not Found" (PcGrade doesn't exist for this FPS year — the worker skips these rows,
    /// it never inserts). Unlike FEC/AGRUP, Staff rates are update-only: there is no
    /// "Inserted"/"Deleted" status because the worker can do neither.
    /// </summary>
    public class BulkRatesStagingStaffRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string PcGrade { get; set; } = string.Empty;
        public decimal? PayRate { get; set; }
        public decimal? PayRateNew { get; set; }
        public decimal? Npr { get; set; }
        public decimal? NprNew { get; set; }
        public decimal? Ohr { get; set; }
        public decimal? OhrNew { get; set; }
    }
}
