namespace Apha.Common.Contracts.FPS
{
    // Source: fps.staffjob_log table + initializeStaffPlanChangesTable() in projectaudit_trail.js
    public class StaffJobLogRes
    {
        public string StaffId { get; set; } = null!;

        public string? Name { get; set; }

        public string JobCode { get; set; } = null!;

        public double PlannedHours { get; set; }

        public DateTime? DateTime { get; set; }

        public string? UserId { get; set; }

        public string? UserEmail { get; set; }

        public string? InsertDelete { get; set; }
    }
}
