namespace Apha.FPS.Core.Entities
{
    public partial class FpsSetting
    {
        public string Id { get; set; } = null!;
        public string? Setting { get; set; }
        public string? Notes { get; set; }        
        public int? FpsYear { get; set; }
        public string? UpdatedBy { get; set; }      
        public DateTime UpdatedAt { get; set; }
    }
}


