namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for creating or updating a Grade record.
    /// Contains only the writable ControlSource-bound fields from frmMaintGrade.
    /// </summary>
    public class GradeReq
    {
        /// <summary>Grade code (primary key). Required.</summary>
        public string GradeCode { get; set; } = null!;

        /// <summary>Grade description. Maps to desc_long column.</summary>
        public string? Description { get; set; }

        /// <summary>Average salary. Maps to avsalary column.</summary>
        public decimal? AvSalary { get; set; }
    }
}
