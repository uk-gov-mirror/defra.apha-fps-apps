namespace Apha.PACT.Core.Entities
{
    public partial class StagingMonthlyTime
    {
        public int Id { get; set; }
        public string? PactStaffId { get; set; }
        public string? TimeCode { get; set; }
        public string? ParentProject { get; set; }
        public double? Month { get; set; }
        public string? WorkGroup { get; set; }
        public double? Hours { get; set; }
        public string? FailureComments { get; set; }
        public bool? Passed { get; set; }
        public string? PactId { get; set; }
        public string? NewWorkGroup { get; set; }
        public string? OldTestCode { get; set; }
        public string? Name { get; set; }
        public string? Filename { get; set; }
        public string? ImportedBy { get; set; }
        public DateTime? ImportedDate { get; set; }
    }
}