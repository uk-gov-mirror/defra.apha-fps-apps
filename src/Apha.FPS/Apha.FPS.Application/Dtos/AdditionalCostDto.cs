namespace Apha.FPS.Application.Dtos
{
    public class AdditionalCostDto
    {
        public string JobCode { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? OriginalDescription { get; set; }

        public string? OriginalAccount { get; set; }

        public decimal ItemCost { get; set; }

        public string? Freq { get; set; }

        public string? Supplier { get; set; }

        public int? FpsYear { get; set; }
    }
}
