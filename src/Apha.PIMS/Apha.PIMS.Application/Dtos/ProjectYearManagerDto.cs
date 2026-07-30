namespace Apha.PIMS.Application.Dtos
{
    /// <summary>
    /// DTO for project year manager details in PIMS service layer
    /// </summary>
    public class ProjectYearManagerDto
    {
        public int? ProjectYear { get; set; }

        public string? ParentProject { get; set; }

        public string? Manager { get; set; }

        public string? ManagerNumber { get; set; }
    }
}
