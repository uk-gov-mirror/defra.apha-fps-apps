namespace Apha.PIMS.Core.Entities
{
    public partial class MabProfitCentre
    {
        public short Year { get; set; }

        public string ProfitCentre { get; set; } = null!;

        public string ProfitCentreName { get; set; } = null!;

        public string Division { get; set; } = null!;

        public decimal? ContTarget { get; set; }

        public string? ProfitCentreHead { get; set; }

        public int? DivisionId { get; set; }
    }
}
