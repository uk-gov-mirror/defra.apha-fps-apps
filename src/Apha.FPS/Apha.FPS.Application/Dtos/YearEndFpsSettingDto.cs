namespace Apha.FPS.Application.Dtos
{
    public class YearEndFpsSettingDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Setting { get; set; }
        public string? Notes { get; set; }
        public int? FpsYear { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ExistsForPlannedYear { get; set; } = string.Empty;
    }
}
