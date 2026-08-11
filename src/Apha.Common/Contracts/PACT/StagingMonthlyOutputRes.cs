namespace Apha.Common.Contracts.PACT
{
    public class StagingMonthlyOutputRes
    {
        public int Id { get; set; }
        public string? TestCode { get; set; }
        public string? Buyer { get; set; }
        public double? Month { get; set; }
        public string? WorkGroup { get; set; }
        public double? Volume { get; set; }
        public string? FailureComments { get; set; }
        public bool? Passed { get; set; }
        public string? Filename { get; set; }
        public string? ImportedBy { get; set; }
        public DateTime? ImportedDate { get; set; }
    }
}
