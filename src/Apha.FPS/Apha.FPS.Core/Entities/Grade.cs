namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Represents a Grade record from fps.grade.
    /// Composite primary key: (GradeCode, FpsYear).
    /// FpsYear is additionally filtered via HasQueryFilter in FpsDbContext.
    /// </summary>
    public partial class Grade
    {
        /// <summary>Grade code (primary key component). Maps to fps.grade.gradecode.</summary>
        public string GradeCode { get; set; } = null!;

        /// <summary>Long description. Maps to fps.grade.desc_long.</summary>
        public string? DescLong { get; set; }

        /// <summary>Average salary. Maps to fps.grade.avsalary.</summary>
        public decimal? AvSalary { get; set; }

        /// <summary>PACT system code. Maps to fps.grade.pactcode.</summary>
        public string? PactCode { get; set; }

        /// <summary>Average leave hours. Maps to fps.grade.avleavehrs.</summary>
        public double? AvLeaveHrs { get; set; }

        /// <summary>Average sick hours. Maps to fps.grade.avsickhrs.</summary>
        public double? AvSickHrs { get; set; }

        /// <summary>FPS financial year (primary key component, filtered by HasQueryFilter). Maps to fps.grade.fpsyear.</summary>
        public int? FpsYear { get; set; }
    }
}
