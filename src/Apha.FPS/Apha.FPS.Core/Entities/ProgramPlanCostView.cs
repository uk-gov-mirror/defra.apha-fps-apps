namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// LINQ result shape for the Program plan hours-cost report
    /// (tlkpProgram × tlkpProject × tblStaffJob × vtblStaff_General × vWorkGroupGrade_General × vProfitCentreGrade_General).
    /// Read-only, not a mapped DB table or view.
    /// </summary>
    public class ProgramPlanCostView
    {
        /// <summary>Computed: "Plan - dd/MM/yyyy".</summary>
        public string? Version { get; set; }
        public string? Directorate { get; set; }
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? Contract { get; set; }
        public string? Project { get; set; }
        public string? Status { get; set; }
        public string? ResourceCentre { get; set; }
        public string? WorkGroup { get; set; }
        public string? GradeCode { get; set; }
        public string? Name { get; set; }
        public double Hours { get; set; }
        /// <summary>Computed: 0 for ZT_prog/ZT_leave/Pend_work programs, otherwise Hours * ChargeRate.</summary>
        public decimal? HoursCost { get; set; }
    }
}
