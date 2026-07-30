namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// DTO for a row in the Resource Management Re-plan grid
    /// (frmRM_RePlan — Section 2 staff grid). Maps from <c>ResourceMgmtReplanView</c>.
    /// </summary>
    public class ResourceMgmtReplanViewDto
    {
        /// <summary>Composite key: "{ParentProject}|{WgGrade}".</summary>
        public string? StaffRowKey { get; set; }

        /// <summary>Workgroup name.</summary>
        public string? WorkGroup { get; set; }

        /// <summary>Grade code.</summary>
        public string? GradeCode { get; set; }

        /// <summary>Workgroup grade identifier.</summary>
        public string? WgGrade { get; set; }

        /// <summary>Full staff name (LastName, FirstName).</summary>
        public string? Name { get; set; }

        /// <summary>Planned hours for the staff–project allocation.</summary>
        public double? PlannedHours { get; set; }

        /// <summary>Parent project code.</summary>
        public string? ParentProject { get; set; }

        /// <summary>Programme number.</summary>
        public string? Program { get; set; }
    }
}
