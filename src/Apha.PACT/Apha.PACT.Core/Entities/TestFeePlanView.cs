namespace Apha.PACT.Core.Entities
{
    /// <summary>
    /// LINQ result shape for the Plan test-fee report (TestOrProduct × tlkpProject × vtblTestRequ × tlkpProgram).
    /// Read-only, not a mapped DB table or view.
    /// </summary>
    public class TestFeePlanView
    {
        /// <summary>Computed: "Plan - dd/MM/yyyy".</summary>
        public string? Version { get; set; }
        public string? Directorate { get; set; }
        public string? Customer { get; set; }
        public string? Program { get; set; }
        public string? Contract { get; set; }
        public string? Project { get; set; }
        public string? Status { get; set; }
        public string TestCode { get; set; } = null!;
        public decimal? UnitPrice { get; set; }
        public double? NoTests { get; set; }
        /// <summary>Computed: NoTests * UnitPrice.</summary>
        public double? TestFee { get; set; }
        public string? Owner { get; set; }
        public int FpsYear { get; set; }
    }
}
