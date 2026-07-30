namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// DTO for a row returned by the Resource Management Re-plan grid
    /// (frmRM_RePlan — Section 2 staff grid).
    /// </summary>
    public class ResourceMgmtReplanViewDto
    {
        /// <summary>Composite key: "{ParentProject}|{WgGrade}". Used as the DataGrid row key.</summary>
        public string? StaffRowKey { get; set; }

        /// <summary>Workgroup name. Maps to <c>tWorkgroupGrade.Workgroup</c>.</summary>
        public string? WorkGroup { get; set; }

        /// <summary>Grade code. Maps to <c>tWorkgroupGrade.GradeCode</c>.</summary>
        public string? GradeCode { get; set; }

        /// <summary>Workgroup grade identifier. Maps to <c>tWorkgroupGrade.WgGrade</c>.</summary>
        public string? WgGrade { get; set; }

        /// <summary>Full staff name (LastName, FirstName).</summary>
        public string? Name { get; set; }

        /// <summary>Planned hours for the staff–project allocation.</summary>
        public double? PlannedHours { get; set; }

        /// <summary>Parent project code. Maps to <c>tlkpProject.ParentProject</c>.</summary>
        public string? ParentProject { get; set; }

        /// <summary>Programme number. Maps to <c>tlkpProject.Program</c>.</summary>
        public string? Program { get; set; }
    }
}
