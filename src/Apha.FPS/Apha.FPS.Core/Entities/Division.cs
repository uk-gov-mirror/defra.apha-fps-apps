using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Division entity mapped to fps.tlkpdivision table.
    /// Represents organizational divisions within agencies for cost allocation and reporting.
    /// </summary>
    [Table("tlkpdivision", Schema = "fps")]
    public partial class Division
    {
        /// <summary>
        /// Division identifier (regular integer field, not auto-generated).
        /// </summary>
        [Column("divisionid")]
        public int? DivisionId { get; set; }

        /// <summary>
        /// Parent agency identifier. References fps.tlkpagency(agencyid).
        /// </summary>
        [Column("agencyid")]
        [Required]
        public int AgencyId { get; set; }

        /// <summary>
        /// Division name. Primary key (case-insensitive text type citext).
        /// </summary>
        [Key]
        [Column("divname")]
        [Required]
        [StringLength(255)]
        public string DivName { get; set; } = null!;

        /// <summary>
        /// Central overhead cost allocation amount.
        /// </summary>
        public decimal? CentOverhead { get; set; }
    }
}
