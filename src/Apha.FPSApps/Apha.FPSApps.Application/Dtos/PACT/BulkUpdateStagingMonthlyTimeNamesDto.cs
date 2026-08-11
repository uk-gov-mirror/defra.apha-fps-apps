namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class BulkUpdateStagingMonthlyTimeNamesDto
    {
        public int? ExcludeId { get; set; }
        public string? OriginalWorkGroup { get; set; }
        public string? OriginalPactStaffId { get; set; }
        public string? NewName { get; set; }
        public string? NewPactStaffId { get; set; }
        public string? NewPactId { get; set; }
    }
}
