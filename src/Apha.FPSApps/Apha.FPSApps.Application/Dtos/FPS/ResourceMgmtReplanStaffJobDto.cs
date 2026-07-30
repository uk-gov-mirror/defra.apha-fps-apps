namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// DTO for a staff-job record used in the all-time and staged panels
    /// of the Resource Management Re-plan screen (frmRM_RePlan).
    /// </summary>
    public class ResourceMgmtReplanStaffJobDto
    {
        /// <summary>Staff identifier. Maps to <c>tEmployee.StaffID</c>.</summary>
        public string? StaffId { get; set; }

        /// <summary>Job code. Maps to <c>tStaffJob.JobCode</c>.</summary>
        public string? JobCode { get; set; }

        /// <summary>Planned hours. Maps to <c>tStaffJob.PlannedHours</c>.</summary>
        public double PlannedHours { get; set; }

        /// <summary>Full staff name (LastName, FirstName).</summary>
        public string? Name { get; set; }

        /// <summary>Workgroup grade. Maps to <c>tWorkgroupGrade.WgGrade</c>.</summary>
        public string? WgGrade { get; set; }

        /// <summary>Grade code. Maps to <c>tWorkgroupGrade.GradeCode</c>.</summary>
        public string? GradeCode { get; set; }

        /// <summary>Workgroup name.</summary>
        public string? WorkGroup { get; set; }

        /// <summary>Charge rate.</summary>
        public decimal? ChargeRate { get; set; }

        /// <summary>Calculated staff cost.</summary>
        public decimal? StaffCost { get; set; }

        /// <summary>Days equivalent of planned hours.</summary>
        public double Days { get; set; }
    }
}
