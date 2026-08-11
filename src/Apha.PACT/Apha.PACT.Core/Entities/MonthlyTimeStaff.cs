namespace Apha.PACT.Core.Entities
{
    public class MonthlyTimeStaff
    {
        public string PactStaffId { get; set; } = null!;

        public string? Name { get; set; }

        public string TimeCode { get; set; } = null!;

        public double Month { get; set; }

        public string ParentProject { get; set; } = null!;

        public string? WorkGroup { get; set; }

        public double? Hours { get; set; }

        public int FpsYear { get; set; }
    }
}
