namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class SettingDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Setting { get; set; }
        public string? Notes { get; set; }
        public int? FpsYear { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
