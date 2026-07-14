using System.ComponentModel.DataAnnotations.Schema;

namespace Apha.FPS.Core.Entities
{
    public partial class StaffJobLog
    {
        public int SequenceNo { get; set; }

        public string StaffId { get; set; } = null!;

        public string JobCode { get; set; } = null!;

        public double PlannedHours { get; set; }

        public DateTime? DateTime { get; set; }

        public string? UserId { get; set; }

        public string? InsertDelete { get; set; }

        public int FpsYear { get; set; }

        // Not a column on fps.staffjob_log; resolved via a lookup against StaffGeneralViews
        // (vtblstaff_general) so the audit trail grid can display the staff member's name.
        [NotMapped]
        public string? Name { get; set; }
    }
}
