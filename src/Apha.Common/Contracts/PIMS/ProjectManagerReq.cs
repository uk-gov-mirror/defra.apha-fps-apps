namespace Apha.Common.Contracts.PIMS
{
    public class ProjectManagerReq
    {
        public string ProjectManager { get; set; } = null!;
        public string? Email { get; set; }
        public string? MNumber { get; set; }
        public bool Disable { get; set; }
        public string? LoginEmail { get; set; }
    }
}
