namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class YearEndSettingDto
    {
        public string Id { get; set; } = string.Empty;
        public string Setting { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int FpsYear { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ExistsForPlannedYear { get; set; } = string.Empty;
    }
}
