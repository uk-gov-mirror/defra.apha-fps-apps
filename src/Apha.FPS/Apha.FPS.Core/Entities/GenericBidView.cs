namespace Apha.FPS.Core.Entities
{
    public partial class GenericBidView
    {
        public string ProfitCentre { get; set; } = null!;

        public string WorkGroupName { get; set; } = null!;

        public string Account { get; set; } = null!;

        public decimal GenBid { get; set; }

        public DateTime? SysTimeStamp { get; set; }

        public string? AccountType { get; set; }
    }
}
