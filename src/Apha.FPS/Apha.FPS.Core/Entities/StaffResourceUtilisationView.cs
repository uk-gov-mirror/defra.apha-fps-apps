namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Represents one aggregated row in the Staff Resource Utilisation query,
    /// grouped by workgroup grade and staff member.
    /// Maps to the columns produced by the resource-utilisation SQL query.
    /// </summary>
    public class StaffResourceUtilisationView
    {
        /// <summary>Workgroup name (wgg.workgroup).</summary>
        public string? WorkGroup { get; set; }

        /// <summary>Profit centre code (pcg.profitcentre).</summary>
        public string? ProfitCentre { get; set; }

        /// <summary>Workgroup grade code (wgg.wggrade).</summary>
        public string? WgGrade { get; set; }

        /// <summary>Staff identifier (s.staffid).</summary>
        public string? StaffId { get; set; }

        /// <summary>Staff member's name — MIN(s.name).</summary>
        public string? Name { get; set; }

        /// <summary>Total paid hours available (s.hrsavail → TotalH in the UI).</summary>
        public double HrsAvail { get; set; }

        /// <summary>Sum of planned hours on ZT programme jobs (plannedzt / ZTW in UI).</summary>
        public double PlannedZt { get; set; }

        /// <summary>Available hours after ZT deduction: hrsavail − plannedzt (Avail in UI).</summary>
        public double AvailSoct { get; set; }

        /// <summary>Sum of planned hours on Not-approved projects (Not-Approved Plan in UI).</summary>
        public double NotApprovedSoct { get; set; }

        /// <summary>Sum of planned hours on Approved projects minus ZT hours (Approved Plan in UI).</summary>
        public double ApprovedSoct { get; set; }

        /// <summary>
        /// Remaining unallocated hours:
        /// availSoct − approvedSoct − notApprovedSoct (Left in UI).
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// Approved utilisation as a percentage of HrsAvail (Approved Util in UI).
        /// Null when HrsAvail = 0.
        /// </summary>
        public double? ApprovedUtilPct { get; set; }

        /// <summary>
        /// Not-approved utilisation as a percentage of HrsAvail (Not-Approved Util in UI).
        /// Null when HrsAvail = 0.
        /// </summary>
        public double? NotApprovedUtilPct { get; set; }

        /// <summary>
        /// Total utilisation (approved + not-approved) as a percentage of HrsAvail (Total Util in UI).
        /// Null when HrsAvail = 0.
        /// </summary>
        public double? TotalUtilPct { get; set; }
    }
}
