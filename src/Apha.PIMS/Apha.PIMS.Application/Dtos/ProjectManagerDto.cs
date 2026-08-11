namespace Apha.PIMS.Application.Dtos
{
    public class ProjectManagerDto
    {
        public string ProjectManager { get; set; } = null!;

        public string? Email { get; set; }

        public string? MNumber { get; set; }

        public bool Disable { get; set; }

        public string? LoginEmail { get; set; }
    }
}
