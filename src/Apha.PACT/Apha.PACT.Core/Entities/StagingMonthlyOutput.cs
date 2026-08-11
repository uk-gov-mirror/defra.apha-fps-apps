namespace Apha.PACT.Core.Entities
{
    public partial class StagingMonthlyOutput
    {
        public int Id { get; set; }
        public string TestCode { get; set; } = null!;

        public string Buyer { get; set; } = null!;

        public double Month { get; set; }

        public string WorkGroup { get; set; } = null!;

        public double? Volume { get; set; }

        public string? FailureComments { get; set; }

        public bool? Passed { get; set; }
        public string? Filename { get; set; }
        public string? ImportedBy { get; set; }
        public DateTime? ImportedDate { get; set; }
    }
}