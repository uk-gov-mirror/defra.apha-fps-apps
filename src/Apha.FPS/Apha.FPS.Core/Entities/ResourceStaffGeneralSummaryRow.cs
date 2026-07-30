namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Result row for the "General" staff summary query (equivalent to the Access
    /// qryZTonly + main GROUP BY query on vtblStaff WHERE Name LIKE '%General').
    /// Columns match the SELECT list of the converted Access query:
    ///   WorkGroupGrade, StaffId, Name, HrsAvail, ZtHours, AppPlannedHours,
    ///   PlannedHours, ChargeHours (= PlannedHrs - ZtHrs),
    ///   AppChargeHours (= AppPlanHrs - ZtHrs).
    /// </summary>
    public class ResourceStaffGeneralSummaryRow
    {
        public string? WorkGroupGrade { get; set; }

        public string? StaffId { get; set; }

        public string? Name { get; set; }

        /// <summary>Hours available for the staff member (vtblStaff.HrsAvail).</summary>
        public double? HrsAvail { get; set; }

        /// <summary>
        /// Total ZT-programme planned hours for the staff member (qryZTonly.SumOfPlannedHours).
        /// Zero when no ZT rows exist (IIf(IsNull(SumOfPlannedHours), 0, SumOfPlannedHours)).
        /// </summary>
        public double ZtHours { get; set; }

        /// <summary>Sum of planned hours on approved projects (Sum(IIf(ProjectStatus='approved', PlannedHours, 0))).</summary>
        public double AppPlannedHours { get; set; }

        /// <summary>Total planned hours across all jobs (Sum(StaffJob.PlannedHours)).</summary>
        public double PlannedHours { get; set; }

        /// <summary>PlannedHours minus ZtHours ([PlanHrs] - [ZThrs]).</summary>
        public double ChargeHours { get; set; }

        /// <summary>AppPlannedHours minus ZtHours ([AppPlanHrs] - [ZThrs]).</summary>
        public double AppChargeHours { get; set; }

        /// <summary>IIf([HrsAvail]=0, "", [PlannedHours]/[HrsAvail]). Null when HrsAvail is 0 or null.</summary>
        public double? Allocation { get; set; }

        /// <summary>IIf([HrsAvail]=0, "", [AppChargeHours]/[HrsAvail]). Null when HrsAvail is 0 or null.</summary>
        public double? Utilization { get; set; }

        /// <summary>IIf([HrsAvail]=0, "", [ChargeHours]/[HrsAvail]). Null when HrsAvail is 0 or null.</summary>
        public double? AppUtilization { get; set; }
    }
}
