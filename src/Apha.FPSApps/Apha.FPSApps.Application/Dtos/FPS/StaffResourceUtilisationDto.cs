namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class StaffResourceUtilisationDto
    {
        public string? WorkGroup { get; set; }
        public string? WgGrade { get; set; }
        public string? StaffId { get; set; }
        public string? Name { get; set; }
        public double HrsAvail { get; set; }
        public double PlannedZt { get; set; }
        public double AvailSoct { get; set; }
        public double NotApprovedSoct { get; set; }
        public double ApprovedSoct { get; set; }
        public double Left { get; set; }
        public double? ApprovedUtilPct { get; set; }
        public double? NotApprovedUtilPct { get; set; }
        public double? TotalUtilPct { get; set; }
    }
}
