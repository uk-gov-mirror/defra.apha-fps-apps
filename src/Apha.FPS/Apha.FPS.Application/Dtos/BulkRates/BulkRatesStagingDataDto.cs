namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// Staged data for a Bulk Rates request's staging grid(s) — "FEC Data (Staging)" +
    /// "Agrup Details" for FEC requests, "Staff Data (Staging)" for Staff requests, or
    /// "Animal Data (Staging)" for Animal requests. Only the relevant list is populated.
    /// </summary>
    public class BulkRatesStagingDataDto
    {
        public IReadOnlyList<BulkRatesStagingFecRowDto> FecRows { get; set; } = [];
        public IReadOnlyList<BulkRatesStagingAgrupRowDto> AgrupRows { get; set; } = [];
        public IReadOnlyList<BulkRatesStagingStaffRowDto> StaffRows { get; set; } = [];
        public IReadOnlyList<BulkRatesStagingAnimalRowDto> AnimalRows { get; set; } = [];
    }
}
