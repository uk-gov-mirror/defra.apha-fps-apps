namespace Apha.FPS.Core.Entities
{
    public partial class YearEndFpsSetting
    {
        public string Id { get; set; } = null!;
        public string? Setting { get; set; }
        public string? Notes { get; set; }        
        public int? FpsYear { get; set; }
        public string? UpdatedBy { get; set; }      
        public DateTime UpdatedAt { get; set; }
        public string ExistsForPlannedYear { get; set; } = "No";
    }
}


