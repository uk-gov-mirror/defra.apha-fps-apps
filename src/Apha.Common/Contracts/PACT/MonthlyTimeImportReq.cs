namespace Apha.Common.Contracts.PACT
{
    public class MonthlyTimeImportRowReq
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

    public class MonthlyTimeImportReq
    {
        public string? FileName { get; set; }
        public short ImportType { get; set; }
        public List<MonthlyTimeImportRowReq> Rows { get; set; } = new();
    }
}
