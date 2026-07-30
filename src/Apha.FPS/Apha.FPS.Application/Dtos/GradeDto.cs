namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Data transfer object for the Grade entity (fps.grade).
    /// Used as the service layer contract between GradeService and API/repository layers.
    /// Composite key: GradeCode + FpsYear (FpsYear enforced via HasQueryFilter in DbContext).
    /// </summary>
    public class GradeDto
    {
        /// <summary>Grade code (primary key component). Maps to fps.grade.gradecode.</summary>
        public string GradeCode { get; set; } = null!;

        /// <summary>Long description. Maps to fps.grade.desc_long (Grade.DescLong).</summary>
        public string? Description { get; set; }

        /// <summary>Average salary. Maps to fps.grade.avsalary.</summary>
        public decimal? AvSalary { get; set; }

        /// <summary>PACT system code. Maps to fps.grade.pactcode.</summary>
        public string? PactCode { get; set; }

        /// <summary>Average leave hours. Maps to fps.grade.avleavehrs.</summary>
        public double? AvLeaveHrs { get; set; }

        /// <summary>Average sick hours. Maps to fps.grade.avsickhrs.</summary>
        public double? AvSickHrs { get; set; }

        /// <summary>FPS financial year (primary key component). Maps to fps.grade.fpsyear.</summary>
        public int? FpsYear { get; set; }
    }
}
