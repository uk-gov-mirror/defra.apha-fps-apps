namespace Apha.Common.Contracts.PACT
{
    public class MonthlyOutputImportRowRes
    {
        public int Id { get; set; }
        public string? WorkGroup { get; set; }
        public string? TestCode { get; set; }
        public string? Buyer { get; set; }
        public string? Month { get; set; }
        public string? Volume { get; set; }
        public bool? Passed { get; set; }
        public string? FailureComments { get; set; }
    }

    public class MonthlyOutputImportRes
    {
        public int ImportedCount { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
