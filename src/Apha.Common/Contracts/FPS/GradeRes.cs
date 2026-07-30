namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a Grade record.
    /// Contains the full RecordSource surface of fps.grade required by CRUD responses.
    /// </summary>
    public class GradeRes
    {
        /// <summary>Grade code (primary key).</summary>
        public string GradeCode { get; set; } = null!;

        /// <summary>Grade description. Maps to desc_long column.</summary>
        public string? Description { get; set; }

        /// <summary>Average salary. Maps to avsalary column.</summary>
        public decimal? AvSalary { get; set; }

        /// <summary>PACT system code. Maps to pactcode column.</summary>
        public string? PactCode { get; set; }

        /// <summary>Average leave hours. Maps to avleavehrs column.</summary>
        public double? AvLeaveHrs { get; set; }

        /// <summary>Average sick hours. Maps to avsickhrs column.</summary>
        public double? AvSickHrs { get; set; }

        /// <summary>FPS financial year (composite primary key, partition key).</summary>
        public int FpsYear { get; set; }
    }
}
