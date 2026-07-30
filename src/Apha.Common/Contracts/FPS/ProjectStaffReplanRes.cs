namespace Apha.Common.Contracts.FPS
{
    public class ProjectStaffReplanRes
    {
        /// <summary>Workgroup name. Maps to <c>tWorkgroupGrade.Workgroup</c>.</summary>
        public string? WorkGroup { get; set; }

        /// <summary>Grade code. Maps to <c>tWorkgroupGrade.GradeCode</c>.</summary>
        public string? GradeCode { get; set; }

        /// <summary>Workgroup grade identifier. Maps to <c>tWorkgroupGrade.WgGrade</c>.</summary>
        public string? WgGrade { get; set; }

        /// <summary>
        /// Full name of the staff member, formatted as "LastName, FirstName".
        /// Derived from <c>tEmployee.LastName</c> and <c>tEmployee.FirstName</c>.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Planned hours for the staff–project allocation.
        /// Maps to <c>tStaffJob.PlannedHours</c>.
        /// </summary>
        public double? PlannedHours { get; set; }

        /// <summary>Parent project code. Maps to <c>tlkpProject.ParentProject</c>.</summary>
        public string? ParentProject { get; set; }

        /// <summary>Programme number. Maps to <c>tlkpProject.Program</c>.</summary>
        public string? Program { get; set; }

        // ── Pagination metadata ───────────────────────────────────────────────

        /// <summary>
        /// Total number of records matching the current filter, used by the frontend
        /// DataGrid for server-side pagination. Populated on list responses only;
        /// defaults to 0 for single-record lookups.
        /// </summary>
        public int TotalCount { get; set; }
    }
}
