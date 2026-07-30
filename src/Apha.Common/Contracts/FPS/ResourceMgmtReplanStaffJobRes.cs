namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// API response contract for a staff-job record used in the all-time and staged panels
    /// of the Resource Management Re-plan screen (frmRM_RePlan).
    /// </summary>
    public class ResourceMgmtReplanStaffJobRes
    {
        /// <summary>Staff identifier.</summary>
        public string? StaffId { get; set; }

        /// <summary>Job code.</summary>
        public string? JobCode { get; set; }

        /// <summary>Planned hours.</summary>
        public double PlannedHours { get; set; }

        /// <summary>Full staff name (LastName, FirstName).</summary>
        public string? Name { get; set; }

        /// <summary>Workgroup grade.</summary>
        public string? WgGrade { get; set; }

        /// <summary>Grade code.</summary>
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
