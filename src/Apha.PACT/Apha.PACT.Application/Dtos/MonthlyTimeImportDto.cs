namespace Apha.PACT.Application.Dtos
{
    public class MonthlyTimeImportRowDto
    {
        public int Id { get; set; }
        public string? WorkGroup { get; set; }
        public string? PactStaffId { get; set; }
        public string? Name { get; set; }
        public string? TimeCode { get; set; }
        public string? ParentProject { get; set; }
        public string? Month { get; set; }
        public string? Hours { get; set; }
        public string? PactId { get; set; }
        public bool? Passed { get; set; }
        public string? FailureComments { get; set; }
    }

    public class MonthlyTimeImportDto
    {
        public string? FileName { get; set; }
        public short ImportType { get; set; }
        public List<MonthlyTimeImportRowDto> Rows { get; set; } = new();
    }

    public class MonthlyTimeImportResultDto
    {
        public int ImportedCount { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
