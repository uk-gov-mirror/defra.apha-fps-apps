namespace Apha.Common.Contracts.PACT
{
    public class SubContractRmsImportRowReq
    {
        public int Id { get; set; }
        public string? Project { get; set; }
        public string? TestJob { get; set; }
        public string? Month { get; set; }
        public string? Amount { get; set; }
        public string? WorkGroup { get; set; }
        public string? AcctCode { get; set; }
        public string? Supplier { get; set; }
        public string? Description { get; set; }
        public string? SupplierNumber { get; set; }
        public string? DailyRate { get; set; }
        public string? AnimalDays { get; set; }
        public string? ValidationFailure { get; set; }
        public DateTime? ImportedDate { get; set; }
    }

    public class SubContractRmsImportReq
    {
        public string? FileName { get; set; }
        public List<SubContractRmsImportRowReq> Rows { get; set; } = new();
    }
}
