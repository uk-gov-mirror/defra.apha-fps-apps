namespace Apha.FPSApps.Application.Dtos.FPS
{
    // Same shape as backend DTO — 8 columns from fps.staffjob_log audit trail table,
    // plus Name (staff display name resolved server-side via a StaffGeneralViews lookup).
    public class StaffJobLogDto
    {
        public int SequenceNo { get; set; }
        public string StaffId { get; set; } = null!;
        public string? Name { get; set; }
        public string JobCode { get; set; } = null!;
        public double PlannedHours { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public int FpsYear { get; set; }
    }
}
