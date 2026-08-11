namespace Apha.PACT.Application.Dtos
{
    public class MonthlyOutputImportRowDto
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

    public class MonthlyOutputImportDto
    {
        public string? FileName { get; set; }
        public short ImportType { get; set; }
        public List<MonthlyOutputImportRowDto> Rows { get; set; } = new();
    }

    public class MonthlyOutputImportResultDto
    {
        public int ImportedCount { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
