namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Result row for the staff job detail query (vtblStaffJob_RM LEFT JOIN vtlkpProject_General).
    /// Returns one row per job for a given staff member, including project metadata.
    /// </summary>
    public class ResourceStaffJobDetailRow
    {
        public string? StaffId { get; set; }

        public double? PlannedHours { get; set; }

        public string? JobCode { get; set; }

        /// <summary>ParentProject from vtlkpProject_General, aliased as JobDescription.</summary>
        public string? JobDescription { get; set; }

        /// <summary>Program from vtlkpProject_General, aliased as Programme.</summary>
        public string? Programme { get; set; }

        public string? ProjectStatus { get; set; }
    }
}
