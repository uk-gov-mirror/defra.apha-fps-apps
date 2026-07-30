namespace Apha.PACT.Core.Entities
{
    public class SubContractRmsImportRow
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

        public double? ParsedMonth { get; set; }
        public decimal? ParsedAmount { get; set; }
        public int? ParsedSupplierNumber { get; set; }
        public decimal? ParsedDailyRate { get; set; }
        public int? ParsedAnimalDays { get; set; }
    }

    public class SubContractRmsImport
    {
        public string? FileName { get; set; }
        public List<SubContractRmsImportRow> Rows { get; set; } = new();
    }

    public class SubContractRmsImportResult
    {
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
    }
}
