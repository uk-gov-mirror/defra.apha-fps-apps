namespace Apha.FPSApps.Application.Dtos.PIMS
{
    /// <summary>
    /// DTO for project year manager details
    /// </summary>
    public class ProjectYearManagerDto
    {
        public int? ProjectYear { get; set; }

        public string? ParentProject { get; set; }

        public string? Manager { get; set; }

        public string? ManagerNumber { get; set; }
    }
}
