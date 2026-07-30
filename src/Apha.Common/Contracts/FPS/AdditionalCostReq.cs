namespace Apha.Common.Contracts.FPS
{
    public class AdditionalCostReq
    {
        public string JobCode { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? OriginalDescription { get; set; }

        public string? OriginalAccount { get; set; }

        public decimal ItemCost { get; set; }

        public string? Freq { get; set; }

        public string? Supplier { get; set; }
    }
}
