namespace Apha.Common.Contracts.PACT
{
    public class MonthlyOutputImportRowReq
    {
        public int Id { get; set; }
        public string? WorkGroup { get; set; }
        public string? TestCode { get; set; }
        public string? ItemDescription { get; set; }
        public string? Buyer { get; set; }
        public string? Month { get; set; }
        public string? Volume { get; set; }
        public bool? Passed { get; set; }
        public string? FailureComments { get; set; }
    }

    public class MonthlyOutputImportReq
    {
        public string? FileName { get; set; }
        public short ImportType { get; set; }
        public List<MonthlyOutputImportRowReq> Rows { get; set; } = new();
    }
}
